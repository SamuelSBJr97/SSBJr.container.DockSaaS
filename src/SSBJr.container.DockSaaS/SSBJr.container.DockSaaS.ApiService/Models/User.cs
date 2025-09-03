using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SSBJr.container.DockSaaS.ApiService.Models;

public class User : IdentityUser<Guid>
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? AvatarUrl { get; set; }
    
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public string FullName => $"{FirstName} {LastName}";
    
    // Navigation properties
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

public class Role : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}