using System.ComponentModel.DataAnnotations;

namespace SSBJr.DockSaaS.ApiService.Models;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }
    
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    public Guid? EntityId { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(20)]
    public string Level { get; set; } = "Info"; // Info, Warning, Error, Critical
}