using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SSBJr.container.DockSaaS.ApiService.Data;
using SSBJr.container.DockSaaS.ApiService.Models;
using SSBJr.container.DockSaaS.ApiService.Services;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework with PostgreSQL
builder.Services.AddDbContext<DockSaaSDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("docksaasdb") 
                          ?? builder.Configuration.GetConnectionString("postgres")
                          ?? builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("No database connection string found. Ensure Aspire is running or a fallback connection string is configured.");
    }
    
    options.UseNpgsql(connectionString);
});

// Configure Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DockSaaSDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
});

// Register application services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IServiceManager, ServiceManager>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IServiceProvisioningService, ServiceProvisioningService>();

// Register service providers
builder.Services.AddScoped<SSBJr.container.DockSaaS.ApiService.Services.IServiceProvider, S3StorageServiceProvider>();
builder.Services.AddScoped<SSBJr.container.DockSaaS.ApiService.Services.IServiceProvider, RDSDatabaseServiceProvider>();
builder.Services.AddScoped<SSBJr.container.DockSaaS.ApiService.Services.IServiceProvider, NoSQLDatabaseServiceProvider>();
builder.Services.AddScoped<IServiceInstanceProviders, ServiceInstanceProviders>();

// Configure background services for metrics collection and data generation
builder.Services.AddHostedService<MetricsCollectionService>();
builder.Services.AddHostedService<BillingProcessingService>();
builder.Services.AddHostedService<NotificationService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("api_service", () => HealthCheckResult.Healthy("API service is running"))
    .AddDbContextCheck<DockSaaSDbContext>("database");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            policy.WithOrigins(
                    "https://localhost:7001", 
                    "http://localhost:5201",
                    "https://localhost:7088", // Fallback port from launchSettings
                    "http://localhost:5288"   // Fallback port from launchSettings
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetIsOriginAllowed((host) => true); // Allow any origin in development
        });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DockSaaS API",
        Version = "v1",
        Description = "A comprehensive SaaS platform for managing AWS-like services with multi-tenant architecture, billing, and advanced monitoring capabilities.",
        Contact = new OpenApiContact
        {
            Name = "DockSaaS Team",
            Email = "support@docksaas.com",
            Url = new Uri("https://github.com/SamuelSBJr97/SSBJr.container.DockSaaS")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Configure JWT authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Add API Key authentication
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key authentication. Add 'X-API-Key' header with your API key.",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DockSaaS API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "DockSaaS API Documentation";
        c.DefaultModelsExpandDepth(-1); // Hide schemas section by default
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Ensure database is created and seeded
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DockSaaSDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var skipOnFailure = configuration.GetValue<bool>("DatabaseInitialization:SkipOnConnectionFailure", true);
    var retryCount = configuration.GetValue<int>("DatabaseInitialization:RetryCount", 3);
    var retryDelay = configuration.GetValue<TimeSpan>("DatabaseInitialization:RetryDelay", TimeSpan.FromSeconds(5));

    for (int attempt = 1; attempt <= retryCount; attempt++)
    {
        try
        {
            logger.LogInformation("Starting database initialization attempt {Attempt}/{MaxAttempts}...", attempt, retryCount);
            
            // Check database connectivity first
            await context.Database.CanConnectAsync();
            logger.LogInformation("Database connection successful");
            
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed");

            // Seed roles
            string[] roles = { "Admin", "Manager", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = role,
                        Description = $"{role} role with specific permissions"
                    });
                }
            }

            // Seed default admin user and tenant
            await SeedDefaultAdminAsync(context, userManager, logger);

            // Seed service definitions
            await SeedServiceDefinitionsAsync(context);

            // Seed pricing plans
            await SeedPricingPlansAsync(context);

            Log.Information("Database initialization completed successfully");
            break; // Success, exit retry loop
        }
        catch (Exception ex)
        {
            var isLastAttempt = attempt == retryCount;
            var isConnectionError = ex.InnerException is System.Net.Sockets.SocketException;
            
            Log.Error(ex, "Database initialization failed on attempt {Attempt}/{MaxAttempts}: {ErrorMessage}", 
                attempt, retryCount, ex.Message);
            
            if (isConnectionError)
            {
                Log.Error("PostgreSQL connection failed. If using Aspire, ensure it's running with: dotnet run --project SSBJr.container.DockSaaS.AppHost");
                Log.Error("If running standalone, ensure PostgreSQL is installed and running on localhost:5432");
            }

            if (isLastAttempt)
            {
                if (skipOnFailure && isConnectionError)
                {
                    Log.Warning("Skipping database initialization due to connection failure. Application will start but database features will not work.");
                    break;
                }
                else
                {
                    throw;
                }
            }
            else
            {
                Log.Information("Retrying database initialization in {Delay} seconds...", retryDelay.TotalSeconds);
                await Task.Delay(retryDelay);
            }
        }
    }
}

app.Run();

// Seeding methods
static async Task SeedDefaultAdminAsync(DockSaaSDbContext context, UserManager<User> userManager, Microsoft.Extensions.Logging.ILogger logger)
{
    try
    {
        // Check if there's already a default tenant
        var defaultTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Name == "DockSaaS");
        
        if (defaultTenant == null)
        {
            defaultTenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "DockSaaS",
                Description = "Default DockSaaS tenant for administration",
                Plan = "Enterprise",
                UserLimit = 100,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Tenants.Add(defaultTenant);
            await context.SaveChangesAsync();
            logger.LogInformation("Default tenant 'DockSaaS' created");
        }

        // Check if admin user already exists
        var adminUser = await userManager.FindByEmailAsync("admin@docksaas.com");
        
        if (adminUser == null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "admin@docksaas.com",
                Email = "admin@docksaas.com",
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                TenantId = defaultTenant.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Default admin user created: admin@docksaas.com / Admin123!");
                logger.LogWarning("??  IMPORTANT: Change the default admin password in production!");
            }
            else
            {
                logger.LogError("Failed to create default admin user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Default admin user already exists");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to seed default admin user");
    }
}

static async Task SeedServiceDefinitionsAsync(DockSaaSDbContext context)
{
    if (!context.ServiceDefinitions.Any())
    {
        var serviceDefinitions = new[]
        {
            new ServiceDefinition
            {
                Id = Guid.NewGuid(),
                Name = "S3-like Storage",
                Type = "S3Storage",
                Description = "Object storage service with buckets, versioning, and lifecycle management",
                ConfigurationSchema = """
                {
                    "bucketName": { "type": "string", "required": true },
                    "encryption": { "type": "boolean", "default": true },
                    "versioning": { "type": "boolean", "default": false },
                    "publicAccess": { "type": "boolean", "default": false }
                }
                """,
                DefaultConfiguration = """
                {
                    "bucketName": "my-bucket",
                    "encryption": true,
                    "versioning": false,
                    "publicAccess": false
                }
                """,
                IsActive = true
            },
            new ServiceDefinition
            {
                Id = Guid.NewGuid(),
                Name = "RDS-like Database",
                Type = "RDSDatabase",
                Description = "Managed relational database service with automatic backups",
                ConfigurationSchema = """
                {
                    "engine": { "type": "string", "enum": ["postgresql", "mysql"], "default": "postgresql" },
                    "version": { "type": "string", "default": "15.0" },
                    "maxConnections": { "type": "integer", "default": 100 }
                }
                """,
                DefaultConfiguration = """
                {
                    "engine": "postgresql",
                    "version": "15.0",
                    "maxConnections": 100
                }
                """,
                IsActive = true
            },
            new ServiceDefinition
            {
                Id = Guid.NewGuid(),
                Name = "DynamoDB-like NoSQL",
                Type = "NoSQLDatabase",
                Description = "NoSQL database service with flexible schema and scaling",
                ConfigurationSchema = """
                {
                    "readCapacity": { "type": "integer", "default": 5 },
                    "writeCapacity": { "type": "integer", "default": 5 },
                    "encryption": { "type": "boolean", "default": true }
                }
                """,
                DefaultConfiguration = """
                {
                    "readCapacity": 5,
                    "writeCapacity": 5,
                    "encryption": true
                }
                """,
                IsActive = true
            },
            new ServiceDefinition
            {
                Id = Guid.NewGuid(),
                Name = "SQS-like Queue",
                Type = "Queue",
                Description = "Message queuing service for decoupling applications",
                ConfigurationSchema = """
                {
                    "queueType": { "type": "string", "enum": ["standard", "fifo"], "default": "standard" },
                    "retentionPeriod": { "type": "integer", "default": 345600 },
                    "maxReceiveCount": { "type": "integer", "default": 10 }
                }
                """,
                DefaultConfiguration = """
                {
                    "queueType": "standard",
                    "retentionPeriod": 345600,
                    "maxReceiveCount": 10
                }
                """,
                IsActive = true
            },
            new ServiceDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Lambda-like Functions",
                Type = "Function",
                Description = "Serverless compute service for running code without managing servers",
                ConfigurationSchema = """
                {
                    "runtime": { "type": "string", "enum": ["dotnet8", "python3.9", "nodejs18"], "default": "dotnet8" },
                    "timeout": { "type": "integer", "default": 30 },
                    "memory": { "type": "integer", "default": 128 }
                }
                """,
                DefaultConfiguration = """
                {
                    "runtime": "dotnet8",
                    "timeout": 30,
                    "memory": 128
                }
                """,
                IsActive = true
            }
        };

        context.ServiceDefinitions.AddRange(serviceDefinitions);
        await context.SaveChangesAsync();
    }
}

static async Task SeedPricingPlansAsync(DockSaaSDbContext context)
{
    // This would be implemented as needed for tenant plan configuration
    await Task.CompletedTask;
}
