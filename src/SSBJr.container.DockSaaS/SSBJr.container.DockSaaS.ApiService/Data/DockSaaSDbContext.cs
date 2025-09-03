using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSBJr.container.DockSaaS.ApiService.Models;

namespace SSBJr.container.DockSaaS.ApiService.Data;

public class DockSaaSDbContext : IdentityDbContext<User, Role, Guid>
{
    public DockSaaSDbContext(DbContextOptions<DockSaaSDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<ServiceDefinition> ServiceDefinitions { get; set; }
    public DbSet<ServiceInstance> ServiceInstances { get; set; }
    public DbSet<ServiceMetric> ServiceMetrics { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Tenant
        builder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Plan).HasDefaultValue("Free");
            entity.Property(e => e.UserLimit).HasDefaultValue(10);
            entity.Property(e => e.StorageLimit).HasDefaultValue(1073741824L);
            entity.Property(e => e.ApiCallsLimit).HasDefaultValue(10000);
        });

        // Configure User
        builder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Email, e.TenantId }).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.Users)
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ServiceDefinition
        builder.Entity<ServiceDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ConfigurationSchema).HasColumnType("jsonb");
            entity.Property(e => e.DefaultConfiguration).HasColumnType("jsonb");
        });

        // Configure ServiceInstance
        builder.Entity<ServiceInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Name, e.TenantId }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Configuration).HasColumnType("jsonb");
            entity.Property(e => e.Status).HasDefaultValue("Created");
            
            entity.HasOne(e => e.ServiceDefinition)
                  .WithMany(sd => sd.ServiceInstances)
                  .HasForeignKey(e => e.ServiceDefinitionId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.ServiceInstances)
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ServiceMetric
        builder.Entity<ServiceMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ServiceInstanceId, e.MetricName, e.Timestamp });
            entity.Property(e => e.MetricName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Tags).HasColumnType("jsonb");
            
            entity.HasOne(e => e.ServiceInstance)
                  .WithMany(si => si.Metrics)
                  .HasForeignKey(e => e.ServiceInstanceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Timestamp });
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
            entity.Property(e => e.Level).HasDefaultValue("Info");
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.AuditLogs)
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed default service definitions
        SeedServiceDefinitions(builder);
    }

    private void SeedServiceDefinitions(ModelBuilder builder)
    {
        var serviceDefinitions = new[]
        {
            new ServiceDefinition
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Name = "S3-like Storage",
                Type = "S3Storage",
                Description = "Object storage service similar to AWS S3",
                IconUrl = "/icons/storage.svg",
                ConfigurationSchema = """
                {
                    "type": "object",
                    "properties": {
                        "bucketName": {"type": "string", "required": true},
                        "region": {"type": "string", "default": "us-east-1"},
                        "encryption": {"type": "boolean", "default": false},
                        "versioning": {"type": "boolean", "default": false}
                    }
                }
                """,
                DefaultConfiguration = """
                {
                    "bucketName": "",
                    "region": "us-east-1",
                    "encryption": false,
                    "versioning": false
                }
                """
            },
            new ServiceDefinition
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Name = "RDS-like Database",
                Type = "RDSDatabase",
                Description = "Relational database service similar to AWS RDS",
                IconUrl = "/icons/database.svg",
                ConfigurationSchema = """
                {
                    "type": "object",
                    "properties": {
                        "engine": {"type": "string", "enum": ["postgresql", "mysql", "sqlserver"], "default": "postgresql"},
                        "instanceClass": {"type": "string", "default": "db.t3.micro"},
                        "allocatedStorage": {"type": "integer", "default": 20},
                        "multiAZ": {"type": "boolean", "default": false}
                    }
                }
                """,
                DefaultConfiguration = """
                {
                    "engine": "postgresql",
                    "instanceClass": "db.t3.micro",
                    "allocatedStorage": 20,
                    "multiAZ": false
                }
                """
            },
            new ServiceDefinition
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Name = "DynamoDB-like NoSQL",
                Type = "NoSQLDatabase",
                Description = "NoSQL database service similar to AWS DynamoDB",
                IconUrl = "/icons/nosql.svg",
                ConfigurationSchema = """
                {
                    "type": "object",
                    "properties": {
                        "tableName": {"type": "string", "required": true},
                        "billingMode": {"type": "string", "enum": ["PROVISIONED", "PAY_PER_REQUEST"], "default": "PAY_PER_REQUEST"},
                        "readCapacity": {"type": "integer", "default": 5},
                        "writeCapacity": {"type": "integer", "default": 5}
                    }
                }
                """,
                DefaultConfiguration = """
                {
                    "tableName": "",
                    "billingMode": "PAY_PER_REQUEST",
                    "readCapacity": 5,
                    "writeCapacity": 5
                }
                """
            },
            new ServiceDefinition
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                Name = "SQS-like Queue",
                Type = "Queue",
                Description = "Message queue service similar to AWS SQS",
                IconUrl = "/icons/queue.svg",
                ConfigurationSchema = """
                {
                    "type": "object",
                    "properties": {
                        "queueName": {"type": "string", "required": true},
                        "visibilityTimeout": {"type": "integer", "default": 30},
                        "messageRetention": {"type": "integer", "default": 1209600},
                        "delaySeconds": {"type": "integer", "default": 0}
                    }
                }
                """,
                DefaultConfiguration = """
                {
                    "queueName": "",
                    "visibilityTimeout": 30,
                    "messageRetention": 1209600,
                    "delaySeconds": 0
                }
                """
            },
            new ServiceDefinition
            {
                Id = new Guid("55555555-5555-5555-5555-555555555555"),
                Name = "Lambda-like Functions",
                Type = "Function",
                Description = "Serverless function service similar to AWS Lambda",
                IconUrl = "/icons/function.svg",
                ConfigurationSchema = """
                {
                    "type": "object",
                    "properties": {
                        "functionName": {"type": "string", "required": true},
                        "runtime": {"type": "string", "enum": ["dotnet8", "node18", "python3.9"], "default": "dotnet8"},
                        "timeout": {"type": "integer", "default": 30},
                        "memory": {"type": "integer", "default": 128}
                    }
                }
                """,
                DefaultConfiguration = """
                {
                    "functionName": "",
                    "runtime": "dotnet8",
                    "timeout": 30,
                    "memory": 128
                }
                """
            }
        };

        builder.Entity<ServiceDefinition>().HasData(serviceDefinitions);
    }
}