using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SSBJr.DockSaaS.ApiService.Data;
using SSBJr.DockSaaS.ApiService.Models;

namespace SSBJr.DockSaaS.ApiService.Services;

public interface IServiceManager
{
    Task<ServiceInstance> CreateServiceInstanceAsync(ServiceInstance serviceInstance);
    Task<ServiceInstance> UpdateServiceInstanceAsync(ServiceInstance serviceInstance);
    Task<bool> DeleteServiceInstanceAsync(Guid serviceInstanceId);
    Task<string> GenerateEndpointUrlAsync(ServiceInstance serviceInstance);
    Task<string> GenerateApiKeyAsync(ServiceInstance serviceInstance);
    Task<bool> StartServiceAsync(Guid serviceInstanceId);
    Task<bool> StopServiceAsync(Guid serviceInstanceId);
    Task<Dictionary<string, object>> GetServiceStatusAsync(Guid serviceInstanceId);
}

public class ServiceManager : IServiceManager
{
    private readonly DockSaaSDbContext _context;
    private readonly ILogger<ServiceManager> _logger;
    private readonly IConfiguration _configuration;

    public ServiceManager(DockSaaSDbContext context, ILogger<ServiceManager> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ServiceInstance> CreateServiceInstanceAsync(ServiceInstance serviceInstance)
    {
        try
        {
            // Generate endpoint URL and API key
            serviceInstance.EndpointUrl = await GenerateEndpointUrlAsync(serviceInstance);
            serviceInstance.ApiKey = await GenerateApiKeyAsync(serviceInstance);
            serviceInstance.Status = "Created";

            _context.ServiceInstances.Add(serviceInstance);
            await _context.SaveChangesAsync();

            // Initialize the service based on its type
            await InitializeServiceAsync(serviceInstance);

            _logger.LogInformation("Service instance {ServiceName} created successfully for tenant {TenantId}", 
                serviceInstance.Name, serviceInstance.TenantId);

            return serviceInstance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create service instance {ServiceName} for tenant {TenantId}", 
                serviceInstance.Name, serviceInstance.TenantId);
            throw;
        }
    }

    public async Task<ServiceInstance> UpdateServiceInstanceAsync(ServiceInstance serviceInstance)
    {
        try
        {
            serviceInstance.UpdatedAt = DateTime.UtcNow;
            _context.ServiceInstances.Update(serviceInstance);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service instance {ServiceInstanceId} updated successfully", serviceInstance.Id);
            return serviceInstance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update service instance {ServiceInstanceId}", serviceInstance.Id);
            throw;
        }
    }

    public async Task<bool> DeleteServiceInstanceAsync(Guid serviceInstanceId)
    {
        try
        {
            var serviceInstance = await _context.ServiceInstances.FindAsync(serviceInstanceId);
            if (serviceInstance == null)
                return false;

            // Stop the service before deletion
            await StopServiceAsync(serviceInstanceId);

            _context.ServiceInstances.Remove(serviceInstance);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service instance {ServiceInstanceId} deleted successfully", serviceInstanceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete service instance {ServiceInstanceId}", serviceInstanceId);
            throw;
        }
    }

    public async Task<string> GenerateEndpointUrlAsync(ServiceInstance serviceInstance)
    {
        var baseUrl = _configuration["ServiceEndpoints:BaseUrl"] ?? "https://api.docksaas.local";
        var servicePath = serviceInstance.ServiceDefinition?.Type?.ToLower() ?? "unknown";
        
        return await Task.FromResult($"{baseUrl}/{servicePath}/{serviceInstance.TenantId}/{serviceInstance.Id}");
    }

    public async Task<string> GenerateApiKeyAsync(ServiceInstance serviceInstance)
    {
        var keyPrefix = serviceInstance.ServiceDefinition?.Type?.ToUpper() ?? "SRV";
        var randomKey = Guid.NewGuid().ToString("N")[..16];
        return await Task.FromResult($"{keyPrefix}_{randomKey}_{serviceInstance.TenantId.ToString("N")[..8]}");
    }

    public async Task<bool> StartServiceAsync(Guid serviceInstanceId)
    {
        try
        {
            var serviceInstance = await _context.ServiceInstances
                .Include(si => si.ServiceDefinition)
                .FirstOrDefaultAsync(si => si.Id == serviceInstanceId);

            if (serviceInstance == null)
                return false;

            // Simulate service startup based on type
            await SimulateServiceStartupAsync(serviceInstance);

            serviceInstance.Status = "Running";
            serviceInstance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service instance {ServiceInstanceId} started successfully", serviceInstanceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start service instance {ServiceInstanceId}", serviceInstanceId);
            
            // Update status to error
            var serviceInstance = await _context.ServiceInstances.FindAsync(serviceInstanceId);
            if (serviceInstance != null)
            {
                serviceInstance.Status = "Error";
                serviceInstance.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            
            return false;
        }
    }

    public async Task<bool> StopServiceAsync(Guid serviceInstanceId)
    {
        try
        {
            var serviceInstance = await _context.ServiceInstances.FindAsync(serviceInstanceId);
            if (serviceInstance == null)
                return false;

            serviceInstance.Status = "Stopped";
            serviceInstance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service instance {ServiceInstanceId} stopped successfully", serviceInstanceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop service instance {ServiceInstanceId}", serviceInstanceId);
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetServiceStatusAsync(Guid serviceInstanceId)
    {
        var serviceInstance = await _context.ServiceInstances
            .Include(si => si.ServiceDefinition)
            .Include(si => si.Metrics.OrderByDescending(m => m.Timestamp).Take(10))
            .FirstOrDefaultAsync(si => si.Id == serviceInstanceId);

        if (serviceInstance == null)
            return new Dictionary<string, object>();

        var status = new Dictionary<string, object>
        {
            ["id"] = serviceInstance.Id,
            ["name"] = serviceInstance.Name,
            ["status"] = serviceInstance.Status,
            ["type"] = serviceInstance.ServiceDefinition?.Type ?? "Unknown",
            ["endpointUrl"] = serviceInstance.EndpointUrl ?? "",
            ["currentUsage"] = serviceInstance.CurrentUsage,
            ["usageQuota"] = serviceInstance.UsageQuota,
            ["usagePercent"] = serviceInstance.UsageQuota > 0 ? (double)serviceInstance.CurrentUsage / serviceInstance.UsageQuota * 100 : 0,
            ["lastAccessed"] = serviceInstance.LastAccessedAt,
            ["uptime"] = serviceInstance.Status == "Running" ? DateTime.UtcNow - serviceInstance.UpdatedAt : TimeSpan.Zero,
            ["metrics"] = serviceInstance.Metrics.Select(m => new
            {
                name = m.MetricName,
                value = m.Value,
                unit = m.Unit,
                timestamp = m.Timestamp
            }).ToList()
        };

        return status;
    }

    private async Task InitializeServiceAsync(ServiceInstance serviceInstance)
    {
        // This method would contain the logic to actually provision the service
        // For now, we'll simulate the initialization
        
        var serviceType = serviceInstance.ServiceDefinition?.Type;
        
        switch (serviceType)
        {
            case "S3Storage":
                await InitializeS3StorageAsync(serviceInstance);
                break;
            case "RDSDatabase":
                await InitializeRDSDatabaseAsync(serviceInstance);
                break;
            case "NoSQLDatabase":
                await InitializeNoSQLDatabaseAsync(serviceInstance);
                break;
            case "Queue":
                await InitializeQueueAsync(serviceInstance);
                break;
            case "Function":
                await InitializeFunctionAsync(serviceInstance);
                break;
            case "Kafka":
                await InitializeKafkaAsync(serviceInstance);
                break;
            default:
                _logger.LogWarning("Unknown service type: {ServiceType}", serviceType);
                break;
        }
    }

    private async Task SimulateServiceStartupAsync(ServiceInstance serviceInstance)
    {
        // Simulate startup time
        await Task.Delay(1000);
        
        // Create initial metrics
        var metrics = new List<ServiceMetric>
        {
            new()
            {
                ServiceInstanceId = serviceInstance.Id,
                MetricName = "cpu_usage",
                Value = Random.Shared.NextDouble() * 100,
                Unit = "percent",
                Timestamp = DateTime.UtcNow
            },
            new()
            {
                ServiceInstanceId = serviceInstance.Id,
                MetricName = "memory_usage",
                Value = Random.Shared.NextDouble() * 100,
                Unit = "percent",
                Timestamp = DateTime.UtcNow
            }
        };

        _context.ServiceMetrics.AddRange(metrics);
        await _context.SaveChangesAsync();
    }

    private async Task InitializeS3StorageAsync(ServiceInstance serviceInstance)
    {
        // Initialize S3-like storage service
        _logger.LogInformation("Initializing S3 Storage service for {ServiceInstanceId}", serviceInstance.Id);
        await Task.Delay(500); // Simulate initialization time
    }

    private async Task InitializeRDSDatabaseAsync(ServiceInstance serviceInstance)
    {
        // Initialize RDS-like database service
        _logger.LogInformation("Initializing RDS Database service for {ServiceInstanceId}", serviceInstance.Id);
        await Task.Delay(2000); // Simulate database creation time
    }

    private async Task InitializeNoSQLDatabaseAsync(ServiceInstance serviceInstance)
    {
        // Initialize NoSQL database service
        _logger.LogInformation("Initializing NoSQL Database service for {ServiceInstanceId}", serviceInstance.Id);
        await Task.Delay(1000); // Simulate table creation time
    }

    private async Task InitializeQueueAsync(ServiceInstance serviceInstance)
    {
        // Initialize queue service
        _logger.LogInformation("Initializing Queue service for {ServiceInstanceId}", serviceInstance.Id);
        await Task.Delay(300); // Simulate queue creation time
    }

    private async Task InitializeFunctionAsync(ServiceInstance serviceInstance)
    {
        // Initialize function service
        _logger.LogInformation("Initializing Function service for {ServiceInstanceId}", serviceInstance.Id);
        await Task.Delay(1500); // Simulate function deployment time
    }

    private async Task InitializeKafkaAsync(ServiceInstance serviceInstance)
    {
        // Initialize Kafka cluster
        _logger.LogInformation("Initializing Kafka cluster for {ServiceInstanceId}", serviceInstance.Id);
        await Task.Delay(800); // Simulate cluster initialization time
        
        // Create default topics
        var defaultTopics = new[] { "events", "logs", "notifications", "metrics" };
        foreach (var topic in defaultTopics)
        {
            _logger.LogDebug("Created default topic {Topic} for service {ServiceInstanceId}", 
                topic, serviceInstance.Id);
        }
    }
}