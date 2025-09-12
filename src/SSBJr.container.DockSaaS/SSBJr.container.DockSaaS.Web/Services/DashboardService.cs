using SSBJr.container.DockSaaS.Web.Models;

namespace SSBJr.container.DockSaaS.Web.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto?> GetDashboardStatsAsync();
    Task<List<AuditLogDto>> GetRecentActivitiesAsync(int limit = 10);
    Task<object?> GetServiceMetricsAsync(Guid serviceInstanceId, int hours = 24);
    Task<object?> GetTenantOverviewAsync();
    Task<object?> GetSystemHealthAsync();
    Task<object?> GetUsageTrendsAsync(int days = 7);
}

public class DashboardService : IDashboardService
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ApiClient apiClient, ILogger<DashboardService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<DashboardStatsDto?> GetDashboardStatsAsync()
    {
        try
        {
            return await _apiClient.GetAsync<DashboardStatsDto>("api/dashboard/stats");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard stats");
            return null;
        }
    }

    public async Task<List<AuditLogDto>> GetRecentActivitiesAsync(int limit = 10)
    {
        try
        {
            var activities = await _apiClient.GetAsync<List<AuditLogDto>>($"api/dashboard/recent-activities?limit={limit}");
            return activities ?? new List<AuditLogDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent activities");
            return new List<AuditLogDto>();
        }
    }

    public async Task<object?> GetServiceMetricsAsync(Guid serviceInstanceId, int hours = 24)
    {
        try
        {
            return await _apiClient.GetAsync<object>($"api/dashboard/service-metrics/{serviceInstanceId}?hours={hours}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service metrics for {ServiceInstanceId}", serviceInstanceId);
            return null;
        }
    }

    public async Task<object?> GetTenantOverviewAsync()
    {
        try
        {
            return await _apiClient.GetAsync<object>("api/dashboard/tenant-overview");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tenant overview");
            return null;
        }
    }

    public async Task<object?> GetSystemHealthAsync()
    {
        try
        {
            return await _apiClient.GetAsync<object>("api/dashboard/system-health");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system health");
            return null;
        }
    }

    public async Task<object?> GetUsageTrendsAsync(int days = 7)
    {
        try
        {
            return await _apiClient.GetAsync<object>($"api/dashboard/usage-trends?days={days}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get usage trends");
            return null;
        }
    }
}