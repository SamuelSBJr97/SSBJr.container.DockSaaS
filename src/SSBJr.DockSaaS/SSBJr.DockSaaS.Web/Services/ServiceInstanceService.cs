using SSBJr.DockSaaS.Web.Models;

namespace SSBJr.DockSaaS.Web.Services;

public interface IServiceInstanceService
{
    Task<List<ServiceDefinitionDto>> GetServiceDefinitionsAsync();
    Task<List<ServiceInstanceDto>> GetServiceInstancesAsync();
    Task<ServiceInstanceDto?> GetServiceInstanceAsync(Guid id);
    Task<ServiceInstanceDto?> CreateServiceInstanceAsync(CreateServiceInstanceRequest request);
    Task<bool> DeleteServiceInstanceAsync(Guid id);
    Task<bool> StartServiceInstanceAsync(Guid id);
    Task<bool> StopServiceInstanceAsync(Guid id);
    Task<Dictionary<string, object>?> GetServiceInstanceStatusAsync(Guid id);
    Task<List<ServiceMetricDto>> GetServiceInstanceMetricsAsync(Guid id, DateTime? from = null, DateTime? to = null);
}

public class ServiceInstanceService : IServiceInstanceService
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<ServiceInstanceService> _logger;

    public ServiceInstanceService(ApiClient apiClient, ILogger<ServiceInstanceService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<ServiceDefinitionDto>> GetServiceDefinitionsAsync()
    {
        try
        {
            var definitions = await _apiClient.GetAsync<List<ServiceDefinitionDto>>("api/services/definitions");
            return definitions ?? new List<ServiceDefinitionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service definitions");
            return new List<ServiceDefinitionDto>();
        }
    }

    public async Task<List<ServiceInstanceDto>> GetServiceInstancesAsync()
    {
        try
        {
            var instances = await _apiClient.GetAsync<List<ServiceInstanceDto>>("api/services/instances");
            return instances ?? new List<ServiceInstanceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service instances");
            return new List<ServiceInstanceDto>();
        }
    }

    public async Task<ServiceInstanceDto?> GetServiceInstanceAsync(Guid id)
    {
        try
        {
            return await _apiClient.GetAsync<ServiceInstanceDto>($"api/services/instances/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service instance {Id}", id);
            return null;
        }
    }

    public async Task<ServiceInstanceDto?> CreateServiceInstanceAsync(CreateServiceInstanceRequest request)
    {
        try
        {
            return await _apiClient.PostAsync<CreateServiceInstanceRequest, ServiceInstanceDto>("api/services/instances", request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create service instance");
            return null;
        }
    }

    public async Task<bool> DeleteServiceInstanceAsync(Guid id)
    {
        try
        {
            return await _apiClient.DeleteAsync($"api/services/instances/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete service instance {Id}", id);
            return false;
        }
    }

    public async Task<bool> StartServiceInstanceAsync(Guid id)
    {
        try
        {
            return await _apiClient.PostAsync($"api/services/instances/{id}/start");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start service instance {Id}", id);
            return false;
        }
    }

    public async Task<bool> StopServiceInstanceAsync(Guid id)
    {
        try
        {
            return await _apiClient.PostAsync($"api/services/instances/{id}/stop");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop service instance {Id}", id);
            return false;
        }
    }

    public async Task<Dictionary<string, object>?> GetServiceInstanceStatusAsync(Guid id)
    {
        try
        {
            return await _apiClient.GetAsync<Dictionary<string, object>>($"api/services/instances/{id}/status");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service instance status {Id}", id);
            return null;
        }
    }

    public async Task<List<ServiceMetricDto>> GetServiceInstanceMetricsAsync(Guid id, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            var endpoint = $"api/services/instances/{id}/metrics";
            var queryParams = new List<string>();

            if (from.HasValue)
                queryParams.Add($"from={from.Value:yyyy-MM-ddTHH:mm:ssZ}");

            if (to.HasValue)
                queryParams.Add($"to={to.Value:yyyy-MM-ddTHH:mm:ssZ}");

            if (queryParams.Any())
                endpoint += "?" + string.Join("&", queryParams);

            var metrics = await _apiClient.GetAsync<List<ServiceMetricDto>>(endpoint);
            return metrics ?? new List<ServiceMetricDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service instance metrics {Id}", id);
            return new List<ServiceMetricDto>();
        }
    }
}