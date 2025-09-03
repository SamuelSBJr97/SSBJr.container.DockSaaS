using System.ComponentModel.DataAnnotations;

namespace SSBJr.container.DockSaaS.ApiService.DTOs;

// Authentication DTOs
public record LoginRequest(
    [Required] string Email,
    [Required] string Password,
    string? TenantName = null
);

public record LoginResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record RegisterRequest(
    [Required] string Email,
    [Required] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string TenantName
);

// User DTOs
public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? AvatarUrl,
    Guid TenantId,
    string TenantName,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool IsActive,
    IList<string> Roles
);

public record CreateUserRequest(
    [Required] string Email,
    [Required] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string Role = "User"
);

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    bool? IsActive
);

// Tenant DTOs
public record TenantDto(
    Guid Id,
    string Name,
    string? Description,
    string? LogoUrl,
    string Plan,
    int UserLimit,
    long StorageLimit,
    int ApiCallsLimit,
    int CurrentUsers,
    long CurrentStorage,
    int CurrentApiCalls,
    DateTime CreatedAt,
    bool IsActive
);

public record CreateTenantRequest(
    [Required] string Name,
    string? Description,
    string? LogoUrl,
    string Plan = "Free"
);

public record UpdateTenantRequest(
    string? Name,
    string? Description,
    string? LogoUrl,
    string? Plan,
    int? UserLimit,
    long? StorageLimit,
    int? ApiCallsLimit,
    bool? IsActive
);

// Service DTOs
public record ServiceDefinitionDto(
    Guid Id,
    string Name,
    string Type,
    string? Description,
    string? IconUrl,
    string ConfigurationSchema,
    string DefaultConfiguration,
    bool IsActive
);

public record ServiceInstanceDto(
    Guid Id,
    string Name,
    Guid ServiceDefinitionId,
    string ServiceDefinitionName,
    string ServiceType,
    Guid TenantId,
    string Configuration,
    string Status,
    string? EndpointUrl,
    string? ApiKey,
    long UsageQuota,
    long CurrentUsage,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastAccessedAt
);

public record CreateServiceInstanceRequest(
    [Required] string Name,
    [Required] Guid ServiceDefinitionId,
    [Required] string Configuration,
    long UsageQuota = 0
);

public record UpdateServiceInstanceRequest(
    string? Name,
    string? Configuration,
    string? Status,
    long? UsageQuota
);

// Audit DTOs
public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string? UserName,
    Guid TenantId,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? Description,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    string? UserAgent,
    DateTime Timestamp,
    string Level
);

// Metrics DTOs
public record ServiceMetricDto(
    Guid Id,
    Guid ServiceInstanceId,
    string MetricName,
    double Value,
    string Unit,
    DateTime Timestamp,
    string? Tags
);

public record DashboardStatsDto(
    int TotalUsers,
    int ActiveServices,
    long TotalStorage,
    int TotalApiCalls,
    double StorageUsagePercent,
    double ApiCallsUsagePercent,
    IList<ServiceMetricDto> RecentMetrics
);