using SSBJr.DockSaaS.ApiService.Models;

namespace SSBJr.DockSaaS.ApiService.Services;

public interface IServiceProvisioningService
{
    Task<ServiceProvisioningResult> ProvisionServiceAsync(ServiceInstance serviceInstance);
    Task<ServiceProvisioningResult> DeprovisionServiceAsync(ServiceInstance serviceInstance);
    Task<ServiceProvisioningResult> StartServiceAsync(ServiceInstance serviceInstance);
    Task<ServiceProvisioningResult> StopServiceAsync(ServiceInstance serviceInstance);
    Task<ServiceStatus> GetServiceStatusAsync(ServiceInstance serviceInstance);
    Task<ServiceMetrics> GetServiceMetricsAsync(ServiceInstance serviceInstance);
}

public class ServiceProvisioningService : IServiceProvisioningService
{
    private readonly ILogger<ServiceProvisioningService> _logger;
    private readonly IServiceInstanceProviders _serviceProviders;
    private readonly IConfiguration _configuration;

    public ServiceProvisioningService(
        ILogger<ServiceProvisioningService> logger,
        IServiceInstanceProviders serviceProviders,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProviders = serviceProviders;
        _configuration = configuration;
    }

    public async Task<ServiceProvisioningResult> ProvisionServiceAsync(ServiceInstance serviceInstance)
    {
        try
        {
            _logger.LogInformation("Provisioning service {ServiceId} of type {ServiceType}", 
                serviceInstance.Id, serviceInstance.ServiceDefinition.Type);

            var provider = _serviceProviders.GetProvider(serviceInstance.ServiceDefinition.Type);
            var result = await provider.ProvisionAsync(serviceInstance);

            if (result.Success)
            {
                serviceInstance.Status = "Running";
                serviceInstance.EndpointUrl = result.EndpointUrl;
                serviceInstance.ApiKey = result.ApiKey;
                serviceInstance.UpdatedAt = DateTime.UtcNow;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision service {ServiceId}", serviceInstance.Id);
            serviceInstance.Status = "Error";
            return new ServiceProvisioningResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ServiceProvisioningResult> DeprovisionServiceAsync(ServiceInstance serviceInstance)
    {
        try
        {
            _logger.LogInformation("Deprovisioning service {ServiceId}", serviceInstance.Id);

            var provider = _serviceProviders.GetProvider(serviceInstance.ServiceDefinition.Type);
            var result = await provider.DeprovisionAsync(serviceInstance);

            if (result.Success)
            {
                serviceInstance.Status = "Stopped";
                serviceInstance.UpdatedAt = DateTime.UtcNow;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deprovision service {ServiceId}", serviceInstance.Id);
            return new ServiceProvisioningResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ServiceProvisioningResult> StartServiceAsync(ServiceInstance serviceInstance)
    {
        try
        {
            var provider = _serviceProviders.GetProvider(serviceInstance.ServiceDefinition.Type);
            var result = await provider.StartAsync(serviceInstance);

            if (result.Success)
            {
                serviceInstance.Status = "Running";
                serviceInstance.UpdatedAt = DateTime.UtcNow;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start service {ServiceId}", serviceInstance.Id);
            return new ServiceProvisioningResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ServiceProvisioningResult> StopServiceAsync(ServiceInstance serviceInstance)
    {
        try
        {
            var provider = _serviceProviders.GetProvider(serviceInstance.ServiceDefinition.Type);
            var result = await provider.StopAsync(serviceInstance);

            if (result.Success)
            {
                serviceInstance.Status = "Stopped";
                serviceInstance.UpdatedAt = DateTime.UtcNow;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop service {ServiceId}", serviceInstance.Id);
            return new ServiceProvisioningResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ServiceStatus> GetServiceStatusAsync(ServiceInstance serviceInstance)
    {
        try
        {
            var provider = _serviceProviders.GetProvider(serviceInstance.ServiceDefinition.Type);
            return await provider.GetStatusAsync(serviceInstance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for service {ServiceId}", serviceInstance.Id);
            return new ServiceStatus
            {
                Status = "Error",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ServiceMetrics> GetServiceMetricsAsync(ServiceInstance serviceInstance)
    {
        try
        {
            var provider = _serviceProviders.GetProvider(serviceInstance.ServiceDefinition.Type);
            return await provider.GetMetricsAsync(serviceInstance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get metrics for service {ServiceId}", serviceInstance.Id);
            return new ServiceMetrics
            {
                ServiceInstanceId = serviceInstance.Id,
                Timestamp = DateTime.UtcNow,
                Metrics = new Dictionary<string, double>
                {
                    ["error"] = 1
                }
            };
        }
    }
}

public class ServiceProvisioningResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? EndpointUrl { get; set; }
    public string? ApiKey { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ServiceStatus
{
    public string Status { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Details { get; set; } = new();
}

public class ServiceMetrics
{
    public Guid ServiceInstanceId { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, double> Metrics { get; set; } = new();
}