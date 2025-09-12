using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SSBJr.DockSaaS.ApiService.Models;

public class ServiceDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // S3Storage, RDSDatabase, NoSQLDatabase, Queue, Function, Monitoring, IAM, Notification
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(255)]
    public string? IconUrl { get; set; }
    
    public string ConfigurationSchema { get; set; } = "{}"; // JSON schema for configuration
    public string DefaultConfiguration { get; set; } = "{}"; // Default configuration JSON
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<ServiceInstance> ServiceInstances { get; set; } = new List<ServiceInstance>();
}

public class ServiceInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public Guid ServiceDefinitionId { get; set; }
    public virtual ServiceDefinition ServiceDefinition { get; set; } = null!;
    
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;
    
    public string Configuration { get; set; } = "{}"; // JSON configuration
    public string Status { get; set; } = "Created"; // Created, Running, Stopped, Error
    
    [MaxLength(255)]
    public string? EndpointUrl { get; set; }
    
    [MaxLength(255)]
    public string? ApiKey { get; set; }
    
    public long UsageQuota { get; set; } = 0; // bytes, requests, etc.
    public long CurrentUsage { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastAccessedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<ServiceMetric> Metrics { get; set; } = new List<ServiceMetric>();
    public virtual ICollection<ServiceMetric> ServiceMetrics { get; set; } = new List<ServiceMetric>();
}

public class ServiceMetric
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ServiceInstanceId { get; set; }
    public virtual ServiceInstance ServiceInstance { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string MetricName { get; set; } = string.Empty;
    
    public double Value { get; set; }
    
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public string? Tags { get; set; } // JSON for additional tags
}