using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var docksaasdb = postgres.AddDatabase("docksaasdb");

// Add Redis cache
var redis = builder.AddRedis("redis")
    .WithDataVolume()
    .WithRedisCommander();

// Add API Service (internal network only)
var apiService = builder.AddProject<Projects.SSBJr_DockSaaS_ApiService>("apiservice")
    .WithReference(docksaasdb)
    .WithReference(redis)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ServiceEndpoints__BaseUrl", "http://apiservice"); // Internal network name

// Add Blazor Web Service (internal network only)
var webService = builder.AddProject<Projects.SSBJr_DockSaaS_Web>("webservice")
    .WithReference(apiService)
    .WithReference(redis)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ApiBaseUrl", "http://apiservice"); // Internal network name

// Configure service discovery
apiService.WithEnvironment("BlazorClientUrls", "http://webservice"); // Internal network name
webService.WithEnvironment("ApiServiceUrl", "http://apiservice"); // Internal network name

var app = builder.Build();

app.Run();
