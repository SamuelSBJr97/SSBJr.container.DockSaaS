using System.ComponentModel.DataAnnotations;

namespace SSBJr.DockSaaS.Web.Models;

// Authentication Models
public class LoginRequest
{
    [Required] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    public string? TenantName { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class RegisterRequest
{
    [Required] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName { get; set; } = "";
    [Required] public string TenantName { get; set; } = "";
}

// User Models
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

// Service Models
public class ServiceDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string ConfigurationSchema { get; set; } = "";
    public string DefaultConfiguration { get; set; } = "";
    public bool IsActive { get; set; }
}

public class ServiceInstanceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid ServiceDefinitionId { get; set; }
    public string ServiceDefinitionName { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public Guid TenantId { get; set; }
    public string Configuration { get; set; } = "";
    public string Status { get; set; } = "";
    public string? EndpointUrl { get; set; }
    public string? ApiKey { get; set; }
    public long UsageQuota { get; set; }
    public long CurrentUsage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}

public class CreateServiceInstanceRequest
{
    [Required] public string Name { get; set; } = "";
    [Required] public Guid ServiceDefinitionId { get; set; }
    [Required] public string Configuration { get; set; } = "";
    public long UsageQuota { get; set; } = 0;
}

// Dashboard Models
public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveServices { get; set; }
    public long TotalStorage { get; set; }
    public int TotalApiCalls { get; set; }
    public double StorageUsagePercent { get; set; }
    public double ApiCallsUsagePercent { get; set; }
    public IList<ServiceMetricDto> RecentMetrics { get; set; } = new List<ServiceMetricDto>();
}

public class ServiceMetricDto
{
    public Guid Id { get; set; }
    public Guid ServiceInstanceId { get; set; }
    public string MetricName { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string? Tags { get; set; }
}

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid TenantId { get; set; }
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public Guid? EntityId { get; set; }
    public string? Description { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "";
}

// API Response Models
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}