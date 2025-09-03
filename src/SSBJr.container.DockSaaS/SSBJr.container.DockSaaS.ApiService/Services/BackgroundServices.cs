using SSBJr.container.DockSaaS.ApiService.Data;
using SSBJr.container.DockSaaS.ApiService.Services;
using Microsoft.EntityFrameworkCore;

namespace SSBJr.container.DockSaaS.ApiService.Services;

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
                var metrics = await provisioningService.GetServiceMetricsAsync(service);
                
                // Record each metric as usage
                foreach (var metric in metrics.Metrics)
                {
                    await billingService.RecordUsageAsync(
                        service.TenantId,
                        service.Id,
                        metric.Key,
                        metric.Value,
                        metrics.Timestamp
                    );
                }

                _logger.LogDebug("Collected metrics for service {ServiceId} ({ServiceType})", 
                    service.Id, service.ServiceDefinition.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect metrics for service {ServiceId}", service.Id);
            }
        }
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

                _logger.LogDebug("Processed billing for tenant {TenantId}", tenant.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process billing for tenant {TenantId}", tenant.Id);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task CleanupOldUsageRecords()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DockSaaSDbContext>();

        // Keep usage records for 90 days
        var cutoffDate = DateTime.UtcNow.AddDays(-90);
        
        var oldRecords = await context.UsageRecords
            .Where(r => r.CreatedAt < cutoffDate)
            .ToListAsync();

        if (oldRecords.Any())
        {
            context.UsageRecords.RemoveRange(oldRecords);
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Cleaned up {Count} old usage records", oldRecords.Count);
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