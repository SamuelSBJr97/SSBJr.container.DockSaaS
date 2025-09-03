var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("docksaas");

// Add Redis cache
var cache = builder.AddRedis("cache");

// Add API service with database and cache dependencies
var apiService = builder.AddProject<Projects.SSBJr_container_DockSaaS_ApiService>("apiservice")
    .WithReference(postgres)
    .WithReference(cache)
    .WaitFor(postgres)
    .WaitFor(cache);

// Add Web frontend with dependencies
builder.AddProject<Projects.SSBJr_container_DockSaaS_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WithReference(apiService)
    .WaitFor(cache)
    .WaitFor(apiService);

builder.Build().Run();
