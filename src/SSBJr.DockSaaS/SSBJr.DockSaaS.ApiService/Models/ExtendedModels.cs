using System.ComponentModel.DataAnnotations;

namespace SSBJr.DockSaaS.ApiService.Models;

public class UsageRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ServiceInstanceId { get; set; }
    public string MetricType { get; set; } = ""; // storage_bytes, api_calls, database_queries, etc.
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Metadata { get; set; } // JSON for additional context

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ServiceInstance ServiceInstance { get; set; } = null!;
}

public class BillingAlert
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string MetricType { get; set; } = "";
    public string AlertLevel { get; set; } = ""; // WARNING, CRITICAL
    public string Message { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
}

public class ServiceTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string ConfigurationTemplate { get; set; } = ""; // JSON template
    public string DefaultValues { get; set; } = ""; // JSON default values
    public string RequiredPlan { get; set; } = "Free"; // Minimum plan required
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? IconUrl { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; } // Comma-separated tags
}

public class ApiKey
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string KeyHash { get; set; } = ""; // Hashed version of the key
    public string KeyPrefix { get; set; } = ""; // First few chars for identification
    public Guid TenantId { get; set; }
    public Guid? ServiceInstanceId { get; set; } // If tied to specific service
    public string[] Scopes { get; set; } = Array.Empty<string>(); // Permissions
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; }
    public string? IpWhitelist { get; set; } // Comma-separated IPs
    public int UsageCount { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ServiceInstance? ServiceInstance { get; set; }
}

public class Notification
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Type { get; set; } = ""; // email, webhook, push
    public string Channel { get; set; } = ""; // email address, webhook URL, etc.
    public string Subject { get; set; } = "";
    public string Content { get; set; } = "";
    public string Priority { get; set; } = "normal"; // low, normal, high, critical
    public string Status { get; set; } = "pending"; // pending, sent, failed, delivered
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public string? Metadata { get; set; } // JSON for additional context

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User? User { get; set; }
}

public class ServiceBackup
{
    public Guid Id { get; set; }
    public Guid ServiceInstanceId { get; set; }
    public string Name { get; set; } = "";
    public string BackupType { get; set; } = ""; // full, incremental, snapshot
    public string Status { get; set; } = ""; // creating, completed, failed
    public long SizeBytes { get; set; }
    public string StorageLocation { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; } // JSON metadata

    // Navigation properties
    public ServiceInstance ServiceInstance { get; set; } = null!;
}

public class TenantSettings
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string SettingKey { get; set; } = "";
    public string SettingValue { get; set; } = "";
    public string DataType { get; set; } = "string"; // string, int, bool, json
    public string? Description { get; set; }
    public bool IsEditable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
}

public class ServiceEvent
{
    public Guid Id { get; set; }
    public Guid ServiceInstanceId { get; set; }
    public string EventType { get; set; } = ""; // started, stopped, error, backup_completed, etc.
    public string EventData { get; set; } = ""; // JSON event details
    public string Severity { get; set; } = "info"; // info, warning, error, critical
    public DateTime Timestamp { get; set; }
    public bool IsProcessed { get; set; }
    public string? ProcessingResult { get; set; }

    // Navigation properties
    public ServiceInstance ServiceInstance { get; set; } = null!;
}

public class TenantInvitation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = "";
    public string Role { get; set; } = "User";
    public string InvitationToken { get; set; } = "";
    public string Status { get; set; } = "pending"; // pending, accepted, expired, cancelled
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public Guid InvitedByUserId { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User InvitedByUser { get; set; } = null!;
}