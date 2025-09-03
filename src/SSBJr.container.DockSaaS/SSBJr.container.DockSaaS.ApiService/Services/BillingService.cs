using SSBJr.container.DockSaaS.ApiService.Models;
using SSBJr.container.DockSaaS.ApiService.Data;
using Microsoft.EntityFrameworkCore;

namespace SSBJr.container.DockSaaS.ApiService.Services;

public interface IBillingService
{
    Task<BillingUsage> GetTenantUsageAsync(Guid tenantId, DateTime fromDate, DateTime toDate);
    Task<decimal> CalculateMonthlyBillAsync(Guid tenantId, int year, int month);
    Task RecordUsageAsync(Guid tenantId, Guid serviceInstanceId, string metricType, double value, DateTime timestamp);
    Task<TenantQuotas> GetTenantQuotasAsync(Guid tenantId);
    Task<bool> CheckQuotaLimitAsync(Guid tenantId, string resourceType, double requestedAmount);
    Task<List<BillingAlert>> GetBillingAlertsAsync(Guid tenantId);
}

public class BillingService : IBillingService
{
    private readonly DockSaaSDbContext _context;
    private readonly ILogger<BillingService> _logger;
    private readonly Dictionary<string, PricingTier> _pricingTiers;

    public BillingService(DockSaaSDbContext context, ILogger<BillingService> logger)
    {
        _context = context;
        _logger = logger;
        _pricingTiers = InitializePricingTiers();
    }

    public async Task<BillingUsage> GetTenantUsageAsync(Guid tenantId, DateTime fromDate, DateTime toDate)
    {
        var usageRecords = await _context.UsageRecords
            .Where(u => u.TenantId == tenantId && u.Timestamp >= fromDate && u.Timestamp <= toDate)
            .ToListAsync();

        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            throw new ArgumentException("Tenant not found");

        var usage = new BillingUsage
        {
            TenantId = tenantId,
            FromDate = fromDate,
            ToDate = toDate,
            Plan = tenant.Plan ?? "Free"
        };

        // Aggregate usage by service type
        var groupedUsage = usageRecords.GroupBy(r => r.MetricType);
        foreach (var group in groupedUsage)
        {
            switch (group.Key)
            {
                case "storage_bytes":
                    usage.StorageUsageBytes = (long)group.Average(g => g.Value); // Average storage over period
                    break;
                case "api_calls":
                    usage.ApiCalls = (int)group.Sum(g => g.Value);
                    break;
                case "database_queries":
                    usage.DatabaseQueries = (int)group.Sum(g => g.Value);
                    break;
                case "function_invocations":
                    usage.FunctionInvocations = (int)group.Sum(g => g.Value);
                    break;
                case "queue_messages":
                    usage.QueueMessages = (int)group.Sum(g => g.Value);
                    break;
            }
        }

        // Calculate costs based on plan
        var pricingTier = _pricingTiers[usage.Plan];
        usage.EstimatedCost = CalculateUsageCost(usage, pricingTier);

        return usage;
    }

    public async Task<decimal> CalculateMonthlyBillAsync(Guid tenantId, int year, int month)
    {
        var fromDate = new DateTime(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        var usage = await GetTenantUsageAsync(tenantId, fromDate, toDate);
        return usage.EstimatedCost;
    }

    public async Task RecordUsageAsync(Guid tenantId, Guid serviceInstanceId, string metricType, double value, DateTime timestamp)
    {
        var usageRecord = new UsageRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ServiceInstanceId = serviceInstanceId,
            MetricType = metricType,
            Value = value,
            Timestamp = timestamp,
            CreatedAt = DateTime.UtcNow
        };

        _context.UsageRecords.Add(usageRecord);
        await _context.SaveChangesAsync();

        // Check if usage exceeds quota limits
        await CheckAndCreateQuotaAlertsAsync(tenantId, metricType, value);
    }

    public async Task<TenantQuotas> GetTenantQuotasAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId);
        if (tenant == null)
            throw new ArgumentException("Tenant not found");

        var plan = tenant.Plan ?? "Free";
        var pricingTier = _pricingTiers[plan];

        // Get current month usage
        var currentMonth = DateTime.UtcNow;
        var fromDate = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        var toDate = fromDate.AddMonths(1);

        var currentUsage = await GetTenantUsageAsync(tenantId, fromDate, toDate);

        return new TenantQuotas
        {
            TenantId = tenantId,
            Plan = plan,
            StorageQuotaBytes = pricingTier.StorageQuotaBytes,
            ApiCallsQuota = pricingTier.ApiCallsQuota,
            DatabaseQueriesQuota = pricingTier.DatabaseQueriesQuota,
            FunctionInvocationsQuota = pricingTier.FunctionInvocationsQuota,
            QueueMessagesQuota = pricingTier.QueueMessagesQuota,
            CurrentStorageUsage = currentUsage.StorageUsageBytes,
            CurrentApiCalls = currentUsage.ApiCalls,
            CurrentDatabaseQueries = currentUsage.DatabaseQueries,
            CurrentFunctionInvocations = currentUsage.FunctionInvocations,
            CurrentQueueMessages = currentUsage.QueueMessages
        };
    }

    public async Task<bool> CheckQuotaLimitAsync(Guid tenantId, string resourceType, double requestedAmount)
    {
        var quotas = await GetTenantQuotasAsync(tenantId);

        return resourceType switch
        {
            "storage_bytes" => quotas.CurrentStorageUsage + (long)requestedAmount <= quotas.StorageQuotaBytes,
            "api_calls" => quotas.CurrentApiCalls + (int)requestedAmount <= quotas.ApiCallsQuota,
            "database_queries" => quotas.CurrentDatabaseQueries + (int)requestedAmount <= quotas.DatabaseQueriesQuota,
            "function_invocations" => quotas.CurrentFunctionInvocations + (int)requestedAmount <= quotas.FunctionInvocationsQuota,
            "queue_messages" => quotas.CurrentQueueMessages + (int)requestedAmount <= quotas.QueueMessagesQuota,
            _ => true
        };
    }

    public async Task<List<BillingAlert>> GetBillingAlertsAsync(Guid tenantId)
    {
        var alerts = await _context.BillingAlerts
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return alerts;
    }

    private Dictionary<string, PricingTier> InitializePricingTiers()
    {
        return new Dictionary<string, PricingTier>
        {
            ["Free"] = new PricingTier
            {
                Name = "Free",
                StorageQuotaBytes = 1024L * 1024 * 1024, // 1GB
                ApiCallsQuota = 1000,
                DatabaseQueriesQuota = 10000,
                FunctionInvocationsQuota = 1000,
                QueueMessagesQuota = 10000,
                StoragePricePerGB = 0m,
                ApiCallPrice = 0m,
                DatabaseQueryPrice = 0m,
                FunctionInvocationPrice = 0m,
                QueueMessagePrice = 0m
            },
            ["Pro"] = new PricingTier
            {
                Name = "Pro",
                StorageQuotaBytes = 100L * 1024 * 1024 * 1024, // 100GB
                ApiCallsQuota = 100000,
                DatabaseQueriesQuota = 1000000,
                FunctionInvocationsQuota = 100000,
                QueueMessagesQuota = 1000000,
                StoragePricePerGB = 0.023m,
                ApiCallPrice = 0.0001m,
                DatabaseQueryPrice = 0.00001m,
                FunctionInvocationPrice = 0.0002m,
                QueueMessagePrice = 0.00001m
            },
            ["Enterprise"] = new PricingTier
            {
                Name = "Enterprise",
                StorageQuotaBytes = 1000L * 1024 * 1024 * 1024, // 1TB
                ApiCallsQuota = 10000000,
                DatabaseQueriesQuota = 100000000,
                FunctionInvocationsQuota = 10000000,
                QueueMessagesQuota = 100000000,
                StoragePricePerGB = 0.020m,
                ApiCallPrice = 0.00008m,
                DatabaseQueryPrice = 0.000008m,
                FunctionInvocationPrice = 0.00015m,
                QueueMessagePrice = 0.000008m
            }
        };
    }

    private decimal CalculateUsageCost(BillingUsage usage, PricingTier pricingTier)
    {
        decimal cost = 0m;

        // Storage cost (per GB)
        var storageGB = usage.StorageUsageBytes / (1024m * 1024m * 1024m);
        cost += storageGB * pricingTier.StoragePricePerGB;

        // API calls cost
        cost += usage.ApiCalls * pricingTier.ApiCallPrice;

        // Database queries cost
        cost += usage.DatabaseQueries * pricingTier.DatabaseQueryPrice;

        // Function invocations cost
        cost += usage.FunctionInvocations * pricingTier.FunctionInvocationPrice;

        // Queue messages cost
        cost += usage.QueueMessages * pricingTier.QueueMessagePrice;

        return Math.Round(cost, 4);
    }

    private async Task CheckAndCreateQuotaAlertsAsync(Guid tenantId, string metricType, double value)
    {
        var quotas = await GetTenantQuotasAsync(tenantId);
        
        var usagePercentage = metricType switch
        {
            "storage_bytes" => quotas.StorageQuotaBytes > 0 ? (double)quotas.CurrentStorageUsage / quotas.StorageQuotaBytes * 100 : 0,
            "api_calls" => quotas.ApiCallsQuota > 0 ? (double)quotas.CurrentApiCalls / quotas.ApiCallsQuota * 100 : 0,
            "database_queries" => quotas.DatabaseQueriesQuota > 0 ? (double)quotas.CurrentDatabaseQueries / quotas.DatabaseQueriesQuota * 100 : 0,
            "function_invocations" => quotas.FunctionInvocationsQuota > 0 ? (double)quotas.CurrentFunctionInvocations / quotas.FunctionInvocationsQuota * 100 : 0,
            "queue_messages" => quotas.QueueMessagesQuota > 0 ? (double)quotas.CurrentQueueMessages / quotas.QueueMessagesQuota * 100 : 0,
            _ => 0
        };

        // Create alerts at 80% and 95% usage
        if (usagePercentage >= 80 && usagePercentage < 95)
        {
            await CreateAlertIfNotExistsAsync(tenantId, metricType, "WARNING", $"{metricType} usage is at {usagePercentage:F1}%");
        }
        else if (usagePercentage >= 95)
        {
            await CreateAlertIfNotExistsAsync(tenantId, metricType, "CRITICAL", $"{metricType} usage is at {usagePercentage:F1}%");
        }
    }

    private async Task CreateAlertIfNotExistsAsync(Guid tenantId, string metricType, string alertLevel, string message)
    {
        var existingAlert = await _context.BillingAlerts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && 
                                     a.MetricType == metricType && 
                                     a.AlertLevel == alertLevel && 
                                     a.IsActive);

        if (existingAlert == null)
        {
            var alert = new BillingAlert
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                MetricType = metricType,
                AlertLevel = alertLevel,
                Message = message,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.BillingAlerts.Add(alert);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Created billing alert for tenant {TenantId}: {Message}", tenantId, message);
        }
    }
}

// Supporting models
public class BillingUsage
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

public class TenantQuotas
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
}

public class PricingTier
{
    public string Name { get; set; } = "";
    public long StorageQuotaBytes { get; set; }
    public int ApiCallsQuota { get; set; }
    public int DatabaseQueriesQuota { get; set; }
    public int FunctionInvocationsQuota { get; set; }
    public int QueueMessagesQuota { get; set; }
    public decimal StoragePricePerGB { get; set; }
    public decimal ApiCallPrice { get; set; }
    public decimal DatabaseQueryPrice { get; set; }
    public decimal FunctionInvocationPrice { get; set; }
    public decimal QueueMessagePrice { get; set; }
}