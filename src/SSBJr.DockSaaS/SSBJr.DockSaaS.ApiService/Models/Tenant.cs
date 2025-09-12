using System.ComponentModel.DataAnnotations;

namespace SSBJr.DockSaaS.ApiService.Models;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(255)]
    public string? LogoUrl { get; set; }
    
    [MaxLength(50)]
    public string Plan { get; set; } = "Free";
    
    public int UserLimit { get; set; } = 10;
    public long StorageLimit { get; set; } = 1073741824; // 1GB in bytes
    public int ApiCallsLimit { get; set; } = 10000;
    
    // Current usage tracking
    public long CurrentStorage { get; set; } = 0;
    public int CurrentApiCalls { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<ServiceInstance> ServiceInstances { get; set; } = new List<ServiceInstance>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}