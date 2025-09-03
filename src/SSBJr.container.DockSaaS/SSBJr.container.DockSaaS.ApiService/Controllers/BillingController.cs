using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSBJr.container.DockSaaS.ApiService.Services;
using SSBJr.container.DockSaaS.ApiService.DTOs;
using System.Security.Claims;

namespace SSBJr.container.DockSaaS.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ILogger<BillingController> _logger;

    public BillingController(IBillingService billingService, ILogger<BillingController> logger)
    {
        _billingService = billingService;
        _logger = logger;
    }

    [HttpGet("usage")]
    public async Task<ActionResult<BillingUsageDto>> GetCurrentUsage(
        [FromQuery] DateTime? fromDate = null, 
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var tenantId = GetTenantId();
            
            // Default to current month if no dates specified
            fromDate ??= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            toDate ??= fromDate.Value.AddMonths(1).AddDays(-1);

            var usage = await _billingService.GetTenantUsageAsync(tenantId, fromDate.Value, toDate.Value);

            return Ok(new BillingUsageDto
            {
                TenantId = usage.TenantId,
                FromDate = usage.FromDate,
                ToDate = usage.ToDate,
                Plan = usage.Plan,
                StorageUsageBytes = usage.StorageUsageBytes,
                ApiCalls = usage.ApiCalls,
                DatabaseQueries = usage.DatabaseQueries,
                FunctionInvocations = usage.FunctionInvocations,
                QueueMessages = usage.QueueMessages,
                EstimatedCost = usage.EstimatedCost
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get billing usage");
            return StatusCode(500, new { message = "Failed to retrieve billing usage" });
        }
    }

    [HttpGet("quotas")]
    public async Task<ActionResult<TenantQuotasDto>> GetQuotas()
    {
        try
        {
            var tenantId = GetTenantId();
            var quotas = await _billingService.GetTenantQuotasAsync(tenantId);

            return Ok(new TenantQuotasDto
            {
                TenantId = quotas.TenantId,
                Plan = quotas.Plan,
                StorageQuotaBytes = quotas.StorageQuotaBytes,
                ApiCallsQuota = quotas.ApiCallsQuota,
                DatabaseQueriesQuota = quotas.DatabaseQueriesQuota,
                FunctionInvocationsQuota = quotas.FunctionInvocationsQuota,
                QueueMessagesQuota = quotas.QueueMessagesQuota,
                CurrentStorageUsage = quotas.CurrentStorageUsage,
                CurrentApiCalls = quotas.CurrentApiCalls,
                CurrentDatabaseQueries = quotas.CurrentDatabaseQueries,
                CurrentFunctionInvocations = quotas.CurrentFunctionInvocations,
                CurrentQueueMessages = quotas.CurrentQueueMessages,
                StorageUsagePercent = quotas.StorageQuotaBytes > 0 ? (double)quotas.CurrentStorageUsage / quotas.StorageQuotaBytes * 100 : 0,
                ApiCallsUsagePercent = quotas.ApiCallsQuota > 0 ? (double)quotas.CurrentApiCalls / quotas.ApiCallsQuota * 100 : 0,
                DatabaseQueriesUsagePercent = quotas.DatabaseQueriesQuota > 0 ? (double)quotas.CurrentDatabaseQueries / quotas.DatabaseQueriesQuota * 100 : 0,
                FunctionInvocationsUsagePercent = quotas.FunctionInvocationsQuota > 0 ? (double)quotas.CurrentFunctionInvocations / quotas.FunctionInvocationsQuota * 100 : 0,
                QueueMessagesUsagePercent = quotas.QueueMessagesQuota > 0 ? (double)quotas.CurrentQueueMessages / quotas.QueueMessagesQuota * 100 : 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tenant quotas");
            return StatusCode(500, new { message = "Failed to retrieve quotas" });
        }
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<List<BillingAlertDto>>> GetAlerts()
    {
        try
        {
            var tenantId = GetTenantId();
            var alerts = await _billingService.GetBillingAlertsAsync(tenantId);

            var alertDtos = alerts.Select(a => new BillingAlertDto
            {
                Id = a.Id,
                MetricType = a.MetricType,
                AlertLevel = a.AlertLevel,
                Message = a.Message,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                ResolvedAt = a.ResolvedAt
            }).ToList();

            return Ok(alertDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get billing alerts");
            return StatusCode(500, new { message = "Failed to retrieve alerts" });
        }
    }

    [HttpGet("bill/{year:int}/{month:int}")]
    public async Task<ActionResult<decimal>> GetMonthlyBill(int year, int month)
    {
        try
        {
            if (month < 1 || month > 12)
                return BadRequest(new { message = "Month must be between 1 and 12" });

            if (year < 2020 || year > DateTime.UtcNow.Year + 1)
                return BadRequest(new { message = "Invalid year" });

            var tenantId = GetTenantId();
            var bill = await _billingService.CalculateMonthlyBillAsync(tenantId, year, month);

            return Ok(new { year, month, amount = bill, currency = "USD" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate monthly bill for {Year}-{Month}", year, month);
            return StatusCode(500, new { message = "Failed to calculate monthly bill" });
        }
    }

    [HttpPost("check-quota")]
    public async Task<ActionResult<bool>> CheckQuotaLimit([FromBody] CheckQuotaRequest request)
    {
        try
        {
            var tenantId = GetTenantId();
            var hasCapacity = await _billingService.CheckQuotaLimitAsync(tenantId, request.ResourceType, request.RequestedAmount);

            return Ok(new { hasCapacity, resourceType = request.ResourceType, requestedAmount = request.RequestedAmount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check quota limit");
            return StatusCode(500, new { message = "Failed to check quota limit" });
        }
    }

    [HttpPost("record-usage")]
    [Authorize(Roles = "Admin")] // Only system admin can manually record usage
    public async Task<ActionResult> RecordUsage([FromBody] RecordUsageRequest request)
    {
        try
        {
            await _billingService.RecordUsageAsync(
                request.TenantId,
                request.ServiceInstanceId,
                request.MetricType,
                request.Value,
                request.Timestamp ?? DateTime.UtcNow
            );

            return Ok(new { message = "Usage recorded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record usage");
            return StatusCode(500, new { message = "Failed to record usage" });
        }
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Invalid or missing tenant information");
        }
        return tenantId;
    }
}

// DTOs for billing endpoints
public class BillingUsageDto
{
    public Guid TenantId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Plan { get; set; } = "";
    public long StorageUsageBytes { get; set; }
    public int ApiCalls { get; set; }
    public int DatabaseQueries { get; set; }
    public int FunctionInvocations { get; set; }
    public int QueueMessages { get; set; }
    public decimal EstimatedCost { get; set; }
}

public class TenantQuotasDto
{
    public Guid TenantId { get; set; }
    public string Plan { get; set; } = "";
    public long StorageQuotaBytes { get; set; }
    public int ApiCallsQuota { get; set; }
    public int DatabaseQueriesQuota { get; set; }
    public int FunctionInvocationsQuota { get; set; }
    public int QueueMessagesQuota { get; set; }
    public long CurrentStorageUsage { get; set; }
    public int CurrentApiCalls { get; set; }
    public int CurrentDatabaseQueries { get; set; }
    public int CurrentFunctionInvocations { get; set; }
    public int CurrentQueueMessages { get; set; }
    public double StorageUsagePercent { get; set; }
    public double ApiCallsUsagePercent { get; set; }
    public double DatabaseQueriesUsagePercent { get; set; }
    public double FunctionInvocationsUsagePercent { get; set; }
    public double QueueMessagesUsagePercent { get; set; }
}

public class BillingAlertDto
{
    public Guid Id { get; set; }
    public string MetricType { get; set; } = "";
    public string AlertLevel { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class CheckQuotaRequest
{
    public string ResourceType { get; set; } = "";
    public double RequestedAmount { get; set; }
}

public class RecordUsageRequest
{
    public Guid TenantId { get; set; }
    public Guid ServiceInstanceId { get; set; }
    public string MetricType { get; set; } = "";
    public double Value { get; set; }
    public DateTime? Timestamp { get; set; }
}