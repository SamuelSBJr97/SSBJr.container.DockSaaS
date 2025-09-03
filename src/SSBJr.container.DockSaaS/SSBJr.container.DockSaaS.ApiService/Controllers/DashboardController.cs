using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSBJr.container.DockSaaS.ApiService.Data;
using SSBJr.container.DockSaaS.ApiService.DTOs;

namespace SSBJr.container.DockSaaS.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DockSaaSDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(DockSaaSDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        return Guid.Parse(tenantIdClaim ?? throw new UnauthorizedAccessException("Tenant ID not found"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            // Get tenant information
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return NotFound("Tenant not found");

            // Get current usage statistics
            var totalUsers = await _context.Users.CountAsync(u => u.TenantId == tenantId && u.IsActive);
            var activeServices = await _context.ServiceInstances.CountAsync(si => si.TenantId == tenantId && si.Status == "Running");
            
            // Calculate storage usage (sum of current usage from all storage services)
            var totalStorage = await _context.ServiceInstances
                .Where(si => si.TenantId == tenantId && si.ServiceDefinition!.Type == "S3Storage")
                .SumAsync(si => si.CurrentUsage);

            // Calculate API calls (simulated based on service instances)
            var totalApiCalls = await _context.ServiceInstances
                .Where(si => si.TenantId == tenantId)
                .SumAsync(si => (int)(si.CurrentUsage / 1000)); // Simplified calculation

            // Calculate usage percentages
            var storageUsagePercent = tenant.StorageLimit > 0 ? (double)totalStorage / tenant.StorageLimit * 100 : 0;
            var apiCallsUsagePercent = tenant.ApiCallsLimit > 0 ? (double)totalApiCalls / tenant.ApiCallsLimit * 100 : 0;

            // Get recent metrics (last 24 hours)
            var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);
            var recentMetrics = await _context.ServiceMetrics
                .Include(sm => sm.ServiceInstance)
                .Where(sm => sm.ServiceInstance.TenantId == tenantId && sm.Timestamp >= twentyFourHoursAgo)
                .OrderByDescending(sm => sm.Timestamp)
                .Take(50)
                .Select(sm => new ServiceMetricDto(
                    sm.Id,
                    sm.ServiceInstanceId,
                    sm.MetricName,
                    sm.Value,
                    sm.Unit,
                    sm.Timestamp,
                    sm.Tags
                ))
                .ToListAsync();

            var stats = new DashboardStatsDto(
                totalUsers,
                activeServices,
                totalStorage,
                totalApiCalls,
                storageUsagePercent,
                apiCallsUsagePercent,
                recentMetrics
            );

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard stats");
            return StatusCode(500, "Failed to get dashboard stats");
        }
    }

    [HttpGet("recent-activities")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetRecentActivities([FromQuery] int limit = 10)
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var activities = await _context.AuditLogs
                .Include(al => al.User)
                .Where(al => al.TenantId == tenantId)
                .OrderByDescending(al => al.Timestamp)
                .Take(limit)
                .Select(al => new AuditLogDto(
                    al.Id,
                    al.UserId,
                    al.User != null ? al.User.FullName : "System",
                    al.TenantId,
                    al.Action,
                    al.EntityType,
                    al.EntityId,
                    al.Description,
                    al.OldValues,
                    al.NewValues,
                    al.IpAddress,
                    al.UserAgent,
                    al.Timestamp,
                    al.Level
                ))
                .ToListAsync();

            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent activities");
            return StatusCode(500, "Failed to get recent activities");
        }
    }

    [HttpGet("service-metrics/{serviceInstanceId}")]
    public async Task<ActionResult<object>> GetServiceMetrics(Guid serviceInstanceId, [FromQuery] int hours = 24)
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            // Verify service belongs to tenant
            var serviceInstance = await _context.ServiceInstances
                .FirstOrDefaultAsync(si => si.Id == serviceInstanceId && si.TenantId == tenantId);

            if (serviceInstance == null)
                return NotFound("Service instance not found");

            var fromTime = DateTime.UtcNow.AddHours(-hours);

            var metrics = await _context.ServiceMetrics
                .Where(sm => sm.ServiceInstanceId == serviceInstanceId && sm.Timestamp >= fromTime)
                .GroupBy(sm => new { sm.MetricName, Hour = sm.Timestamp.Hour })
                .Select(g => new
                {
                    MetricName = g.Key.MetricName,
                    Hour = g.Key.Hour,
                    AverageValue = g.Average(sm => sm.Value),
                    MaxValue = g.Max(sm => sm.Value),
                    MinValue = g.Min(sm => sm.Value),
                    Count = g.Count()
                })
                .OrderBy(m => m.Hour)
                .ToListAsync();

            // Group by metric name for easier charting
            var groupedMetrics = metrics
                .GroupBy(m => m.MetricName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => new
                    {
                        m.Hour,
                        m.AverageValue,
                        m.MaxValue,
                        m.MinValue,
                        m.Count
                    }).ToList()
                );

            return Ok(new
            {
                ServiceInstanceId = serviceInstanceId,
                ServiceName = serviceInstance.Name,
                TimeRange = $"Last {hours} hours",
                Metrics = groupedMetrics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service metrics for {ServiceInstanceId}", serviceInstanceId);
            return StatusCode(500, "Failed to get service metrics");
        }
    }

    [HttpGet("tenant-overview")]
    public async Task<ActionResult<object>> GetTenantOverview()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return NotFound("Tenant not found");

            var servicesSummary = await _context.ServiceInstances
                .Include(si => si.ServiceDefinition)
                .Where(si => si.TenantId == tenantId)
                .GroupBy(si => new { si.ServiceDefinition!.Type, si.Status })
                .Select(g => new
                {
                    ServiceType = g.Key.Type,
                    Status = g.Key.Status,
                    Count = g.Count(),
                    TotalUsage = g.Sum(si => si.CurrentUsage),
                    TotalQuota = g.Sum(si => si.UsageQuota)
                })
                .ToListAsync();

            var usersSummary = await _context.Users
                .Where(u => u.TenantId == tenantId)
                .GroupBy(u => u.IsActive)
                .Select(g => new
                {
                    IsActive = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(new
            {
                Tenant = new
                {
                    tenant.Id,
                    tenant.Name,
                    tenant.Plan,
                    tenant.UserLimit,
                    tenant.StorageLimit,
                    tenant.ApiCallsLimit,
                    tenant.CreatedAt
                },
                Services = servicesSummary.GroupBy(s => s.ServiceType)
                    .ToDictionary(
                        g => g.Key,
                        g => new
                        {
                            Total = g.Sum(s => s.Count),
                            Running = g.Where(s => s.Status == "Running").Sum(s => s.Count),
                            Stopped = g.Where(s => s.Status == "Stopped").Sum(s => s.Count),
                            Error = g.Where(s => s.Status == "Error").Sum(s => s.Count),
                            TotalUsage = g.Sum(s => s.TotalUsage),
                            TotalQuota = g.Sum(s => s.TotalQuota)
                        }
                    ),
                Users = new
                {
                    Total = usersSummary.Sum(u => u.Count),
                    Active = usersSummary.Where(u => u.IsActive).Sum(u => u.Count),
                    Inactive = usersSummary.Where(u => !u.IsActive).Sum(u => u.Count)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tenant overview");
            return StatusCode(500, "Failed to get tenant overview");
        }
    }
}