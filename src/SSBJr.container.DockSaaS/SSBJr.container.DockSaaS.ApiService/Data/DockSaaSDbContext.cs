using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSBJr.container.DockSaaS.ApiService.Models;

namespace SSBJr.container.DockSaaS.ApiService.Data;

public class DockSaaSDbContext : IdentityDbContext<User, Role, Guid>
{
    public DockSaaSDbContext(DbContextOptions<DockSaaSDbContext> options) : base(options)
    {
    }

    // Existing DbSets
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<ServiceDefinition> ServiceDefinitions { get; set; }
    public DbSet<ServiceInstance> ServiceInstances { get; set; }
    public DbSet<ServiceMetric> ServiceMetrics { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    // New DbSets for enhanced functionality
    public DbSet<UsageRecord> UsageRecords { get; set; }
    public DbSet<BillingAlert> BillingAlerts { get; set; }
    public DbSet<ServiceTemplate> ServiceTemplates { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Models.Notification> Notifications { get; set; }
    public DbSet<ServiceBackup> ServiceBackups { get; set; }
    public DbSet<TenantSettings> TenantSettings { get; set; }
    public DbSet<ServiceEvent> ServiceEvents { get; set; }
    public DbSet<TenantInvitation> TenantInvitations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure User entity
        builder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            
            // User-Tenant relationship
            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.Users)
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Role entity
        builder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure Tenant entity
        builder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Plan).HasMaxLength(50).HasDefaultValue("Free");
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure ServiceDefinition entity
        builder.Entity<ServiceDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ConfigurationSchema).HasColumnType("text");
            entity.Property(e => e.DefaultConfiguration).HasColumnType("text");
            entity.HasIndex(e => e.Type).IsUnique();
        });

        // Configure ServiceInstance entity
        builder.Entity<ServiceInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Created");
            entity.Property(e => e.Configuration).HasColumnType("text");
            entity.Property(e => e.EndpointUrl).HasMaxLength(500);
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            
            // ServiceInstance-Tenant relationship
            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.ServiceInstances)
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // ServiceInstance-ServiceDefinition relationship
            entity.HasOne(e => e.ServiceDefinition)
                  .WithMany()
                  .HasForeignKey(e => e.ServiceDefinitionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
        });

        // Configure ServiceMetric entity
        builder.Entity<ServiceMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Tags).HasColumnType("text");
            
            // ServiceMetric-ServiceInstance relationship
            entity.HasOne(e => e.ServiceInstance)
                  .WithMany(s => s.ServiceMetrics)
                  .HasForeignKey(e => e.ServiceInstanceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ServiceInstanceId, e.Timestamp, e.MetricName });
        });

        // Configure AuditLog entity
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.OldValues).HasColumnType("text");
            entity.Property(e => e.NewValues).HasColumnType("text");
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Level).HasMaxLength(50).HasDefaultValue("Info");
            
            // AuditLog-Tenant relationship
            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // AuditLog-User relationship (optional)
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.Timestamp });
            entity.HasIndex(e => e.EntityType);
        });

        // Configure UsageRecord entity
        builder.Entity<UsageRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Metadata).HasColumnType("text");

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceInstance)
                  .WithMany()
                  .HasForeignKey(e => e.ServiceInstanceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.Timestamp, e.MetricType });
        });

        // Configure BillingAlert entity
        builder.Entity<BillingAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AlertLevel).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.IsActive });
        });

        // Configure ServiceTemplate entity
        builder.Entity<ServiceTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ServiceType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ConfigurationTemplate).HasColumnType("text");
            entity.Property(e => e.DefaultValues).HasColumnType("text");
            entity.Property(e => e.RequiredPlan).HasMaxLength(50).HasDefaultValue("Free");
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Tags).HasMaxLength(500);
        });

        // Configure ApiKey entity
        builder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.KeyHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.KeyPrefix).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Scopes).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            entity.Property(e => e.IpWhitelist).HasMaxLength(1000);

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceInstance)
                  .WithMany()
                  .HasForeignKey(e => e.ServiceInstanceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.IsActive });
        });

        // Configure Notification entity
        builder.Entity<Models.Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Channel).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("normal");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.Metadata).HasColumnType("text");

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.Status, e.CreatedAt });
        });

        // Configure ServiceBackup entity
        builder.Entity<ServiceBackup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.BackupType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.StorageLocation).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.Metadata).HasColumnType("text");

            entity.HasOne(e => e.ServiceInstance)
                  .WithMany()
                  .HasForeignKey(e => e.ServiceInstanceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ServiceInstanceId, e.CreatedAt });
        });

        // Configure TenantSettings entity
        builder.Entity<TenantSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SettingKey).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SettingValue).HasColumnType("text").IsRequired();
            entity.Property(e => e.DataType).HasMaxLength(20).HasDefaultValue("string");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.SettingKey }).IsUnique();
        });

        // Configure ServiceEvent entity
        builder.Entity<ServiceEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EventData).HasColumnType("text");
            entity.Property(e => e.Severity).HasMaxLength(20).HasDefaultValue("info");
            entity.Property(e => e.ProcessingResult).HasMaxLength(1000);

            entity.HasOne(e => e.ServiceInstance)
                  .WithMany()
                  .HasForeignKey(e => e.ServiceInstanceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ServiceInstanceId, e.Timestamp });
            entity.HasIndex(e => new { e.IsProcessed, e.Timestamp });
        });

        // Configure TenantInvitation entity
        builder.Entity<TenantInvitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("User");
            entity.Property(e => e.InvitationToken).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InvitedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.InvitedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.InvitationToken).IsUnique();
            entity.HasIndex(e => new { e.Email, e.TenantId, e.Status });
        });
    }
}