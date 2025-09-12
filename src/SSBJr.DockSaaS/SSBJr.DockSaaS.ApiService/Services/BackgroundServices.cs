using SSBJr.DockSaaS.ApiService.Data;
using SSBJr.DockSaaS.ApiService.Services;
using Microsoft.EntityFrameworkCore;
using SSBJr.DockSaaS.ApiService.Models;

namespace SSBJr.DockSaaS.ApiService.Services;

public class MetricsCollectionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MetricsCollectionService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Collect metrics every 5 minutes

    public MetricsCollectionService(IServiceScopeFactory scopeFactory, ILogger<MetricsCollectionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics collection service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectMetricsForAllServices();
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while collecting metrics");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait before retrying
            }
        }

        _logger.LogInformation("Metrics collection service stopped");
    }

    private async Task CollectMetricsForAllServices()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DockSaaSDbContext>();
        var provisioningService = scope.ServiceProvider.GetRequiredService<IServiceProvisioningService>();
        var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();

        var activeServices = await context.ServiceInstances
            .Where(s => s.Status == "Running")
            .Include(s => s.Tenant)
            .Include(s => s.ServiceDefinition)
            .ToListAsync();

        foreach (var service in activeServices)
        {
            try
            {
                // Generate realistic metrics based on service type
                var metrics = GenerateRealisticMetrics(service);
                
                // Record each metric
                foreach (var metric in metrics)
                {
                    var serviceMetric = new ServiceMetric
                    {
                        Id = Guid.NewGuid(),
                        ServiceInstanceId = service.Id,
                        MetricName = metric.Key,
                        Value = metric.Value,
                        Unit = GetMetricUnit(metric.Key),
                        Timestamp = DateTime.UtcNow,
                        Tags = $"{{\"service_type\":\"{service.ServiceDefinition.Type}\",\"tenant_id\":\"{service.TenantId}\"}}"
                    };

                    context.ServiceMetrics.Add(serviceMetric);

                    // Record usage for billing
                    await billingService.RecordUsageAsync(
                        service.TenantId,
                        service.Id,
                        metric.Key,
                        metric.Value,
                        DateTime.UtcNow
                    );
                }

                // Update service current usage (simulate growth)
                service.CurrentUsage += new Random().Next(1, 100);
                service.LastAccessedAt = DateTime.UtcNow;

                _logger.LogDebug("Collected metrics for service {ServiceId} ({ServiceType})", 
                    service.Id, service.ServiceDefinition.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect metrics for service {ServiceId}", service.Id);
            }
        }

        await context.SaveChangesAsync();
    }

    private Dictionary<string, double> GenerateRealisticMetrics(ServiceInstance service)
    {
        var random = new Random();
        var metrics = new Dictionary<string, double>();

        switch (service.ServiceDefinition.Type)
        {
            case "S3Storage":
                metrics["storage_bytes"] = random.NextDouble() * 1000000000; // 0-1GB
                metrics["requests_per_second"] = random.NextDouble() * 100;
                metrics["download_bandwidth_mbps"] = random.NextDouble() * 50;
                metrics["upload_bandwidth_mbps"] = random.NextDouble() * 20;
                break;

            case "RDSDatabase":
                metrics["cpu_utilization"] = random.NextDouble() * 100;
                metrics["memory_utilization"] = random.NextDouble() * 100;
                metrics["connection_count"] = random.Next(1, 50);
                metrics["query_latency_ms"] = random.NextDouble() * 500;
                metrics["disk_io_ops"] = random.Next(10, 1000);
                break;

            case "NoSQLDatabase":
                metrics["read_capacity_units"] = random.NextDouble() * 1000;
                metrics["write_capacity_units"] = random.NextDouble() * 500;
                metrics["throttled_requests"] = random.Next(0, 10);
                metrics["item_count"] = random.Next(1000, 100000);
                break;

            case "Queue":
                metrics["messages_visible"] = random.Next(0, 1000);
                metrics["messages_sent"] = random.Next(10, 500);
                metrics["messages_received"] = random.Next(5, 400);
                metrics["approximate_age_seconds"] = random.NextDouble() * 3600;
                break;

            case "Function":
                metrics["invocations"] = random.Next(1, 100);
                metrics["duration_ms"] = random.NextDouble() * 5000;
                metrics["error_count"] = random.Next(0, 5);
                metrics["memory_used_mb"] = random.NextDouble() * 512;
                break;

            default:
                // Generic metrics for unknown service types
                metrics["cpu_utilization"] = random.NextDouble() * 100;
                metrics["memory_utilization"] = random.NextDouble() * 100;
                metrics["requests_per_second"] = random.NextDouble() * 50;
                break;
        }

        return metrics;
    }

    private string GetMetricUnit(string metricName)
    {
        return metricName switch
        {
            "storage_bytes" => "bytes",
            "requests_per_second" => "rps",
            "download_bandwidth_mbps" => "mbps",
            "upload_bandwidth_mbps" => "mbps",
            "cpu_utilization" => "percent",
            "memory_utilization" => "percent",
            "connection_count" => "count",
            "query_latency_ms" => "ms",
            "disk_io_ops" => "ops",
            "read_capacity_units" => "rcu",
            "write_capacity_units" => "wcu",
            "throttled_requests" => "count",
            "item_count" => "count",
            "messages_visible" => "count",
            "messages_sent" => "count",
            "messages_received" => "count",
            "approximate_age_seconds" => "seconds",
            "invocations" => "count",
            "duration_ms" => "ms",
            "error_count" => "count",
            "memory_used_mb" => "mb",
            _ => "unit"
        };
    }
}

public class BillingProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BillingProcessingService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Process billing every hour

    public BillingProcessingService(IServiceScopeFactory scopeFactory, ILogger<BillingProcessingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Billing processing service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBillingForAllTenants();
                await CleanupOldUsageRecords();
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing billing");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before retrying
            }
        }

        _logger.LogInformation("Billing processing service stopped");
    }

    private async Task ProcessBillingForAllTenants()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DockSaaSDbContext>();
        var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();

        var activeTenants = await context.Tenants
            .Where(t => t.IsActive)
            .ToListAsync();

        foreach (var tenant in activeTenants)
        {
            try
            {
                // Check quota limits and create alerts
                var quotas = await billingService.GetTenantQuotasAsync(tenant.Id);
                
                // Update tenant current usage
                tenant.CurrentStorage = quotas.CurrentStorageUsage;
                tenant.CurrentApiCalls = quotas.CurrentApiCalls;
                tenant.UpdatedAt = DateTime.UtcNow;

                // Generate alerts if approaching limits
                if (tenant.StorageLimit > 0)
                {
                    var storagePercent = (double)tenant.CurrentStorage / tenant.StorageLimit * 100;
                    if (storagePercent > 80)
                    {
                        await CreateBillingAlert(context, tenant.Id, "storage", "warning", 
                            $"Storage usage is at {storagePercent:F1}% of limit");
                    }
                }

                if (tenant.ApiCallsLimit > 0)
                {
                    var apiCallsPercent = (double)tenant.CurrentApiCalls / tenant.ApiCallsLimit * 100;
                    if (apiCallsPercent > 80)
                    {
                        await CreateBillingAlert(context, tenant.Id, "api_calls", "warning", 
                            $"API calls usage is at {apiCallsPercent:F1}% of limit");
                    }
                }

                _logger.LogDebug("Processed billing for tenant {TenantId}", tenant.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process billing for tenant {TenantId}", tenant.Id);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task CreateBillingAlert(DockSaaSDbContext context, Guid tenantId, string metricType, string level, string message)
    {
        // Check if alert already exists and is active
        var existingAlert = await context.BillingAlerts
            .FirstOrDefaultAsync(ba => ba.TenantId == tenantId && 
                                     ba.MetricType == metricType && 
                                     ba.IsActive);

        if (existingAlert == null)
        {
            var alert = new BillingAlert
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MetricType = metricType,
                AlertLevel = level,
                Message = message,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.BillingAlerts.Add(alert);
        }
    }

    private async Task CleanupOldUsageRecords()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DockSaaSDbContext>();

        // Keep usage records for 90 days
        var cutoffDate = DateTime.UtcNow.AddDays(-90);
        
        var oldRecords = await context.UsageRecords
            .Where(r => r.CreatedAt < cutoffDate)
            .Take(1000) // Process in batches
            .ToListAsync();

        if (oldRecords.Any())
        {
            context.UsageRecords.RemoveRange(oldRecords);
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Cleaned up {Count} old usage records", oldRecords.Count);
        }

        // Cleanup old service metrics (keep for 30 days)
        var metricsCutoffDate = DateTime.UtcNow.AddDays(-30);
        var oldMetrics = await context.ServiceMetrics
            .Where(sm => sm.Timestamp < metricsCutoffDate)
            .Take(1000)
            .ToListAsync();

        if (oldMetrics.Any())
        {
            context.ServiceMetrics.RemoveRange(oldMetrics);
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Cleaned up {Count} old service metrics", oldMetrics.Count);
        }
    }
}

public class NotificationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(2); // Process notifications every 2 minutes

    public NotificationService(IServiceScopeFactory scopeFactory, ILogger<NotificationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotifications();
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing notifications");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("Notification service stopped");
    }

    private async Task ProcessPendingNotifications()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DockSaaSDbContext>();

        var pendingNotifications = await context.Notifications
            .Where(n => n.Status == "pending" && n.RetryCount < 3)
            .OrderBy(n => n.CreatedAt)
            .Take(50) // Process in batches
            .ToListAsync();

        foreach (var notification in pendingNotifications)
        {
            try
            {
                await SendNotificationAsync(notification);
                notification.Status = "sent";
                notification.SentAt = DateTime.UtcNow;
                
                _logger.LogDebug("Sent notification {NotificationId} to {Channel}", 
                    notification.Id, notification.Channel);
            }
            catch (Exception ex)
            {
                notification.Status = "failed";
                notification.ErrorMessage = ex.Message;
                notification.RetryCount++;
                
                _logger.LogError(ex, "Failed to send notification {NotificationId}", notification.Id);
            }
        }

        if (pendingNotifications.Any())
        {
            await context.SaveChangesAsync();
        }
    }

    private async Task SendNotificationAsync(Models.Notification notification)
    {
        // Simulate sending notification
        await Task.Delay(100); // Simulate network call
        
        switch (notification.Type.ToLower())
        {
            case "email":
                await SendEmailAsync(notification);
                break;
            case "webhook":
                await SendWebhookAsync(notification);
                break;
            case "push":
                await SendPushNotificationAsync(notification);
                break;
            default:
                throw new NotSupportedException($"Notification type '{notification.Type}' is not supported");
        }
    }

    private async Task SendEmailAsync(Models.Notification notification)
    {
        // Implementation would integrate with email service (SendGrid, AWS SES, etc.)
        _logger.LogInformation("Sending email to {Email}: {Subject}", notification.Channel, notification.Subject);
        await Task.Delay(50); // Simulate email sending
    }

    private async Task SendWebhookAsync(Models.Notification notification)
    {
        // Implementation would make HTTP POST to webhook URL
        _logger.LogInformation("Sending webhook to {Url}", notification.Channel);
        await Task.Delay(50); // Simulate webhook call
    }

    private async Task SendPushNotificationAsync(Models.Notification notification)
    {
        // Implementation would integrate with push notification service
        _logger.LogInformation("Sending push notification to {DeviceId}", notification.Channel);
        await Task.Delay(50); // Simulate push notification
    }
}