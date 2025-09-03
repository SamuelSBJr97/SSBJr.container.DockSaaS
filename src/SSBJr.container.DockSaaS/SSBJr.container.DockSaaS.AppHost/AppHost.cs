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

// Add API Service
var apiService = builder.AddProject<Projects.SSBJr_container_DockSaaS_ApiService>("apiservice")
    .WithReference(docksaasdb)
    .WithReference(redis)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ServiceEndpoints__BaseUrl", "https://localhost:7000")
    .WithHttpsEndpoint(port: 7000, name: "api-https")
    .WithHttpEndpoint(port: 5000, name: "api-http");

// Add Blazor Web Service
var webService = builder.AddProject<Projects.SSBJr_container_DockSaaS_Web>("webservice")
    .WithReference(apiService)
    .WithReference(redis)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ApiBaseUrl", "https://localhost:7000")
    .WithHttpsEndpoint(port: 7001, name: "web-https")
    .WithHttpEndpoint(port: 5001, name: "web-http");

// Configure service discovery
apiService.WithEnvironment("BlazorClientUrls", "https://localhost:7001,http://localhost:5001");
webService.WithEnvironment("ApiServiceUrl", apiService.GetEndpoint("api-https"));

var app = builder.Build();

app.Run();
