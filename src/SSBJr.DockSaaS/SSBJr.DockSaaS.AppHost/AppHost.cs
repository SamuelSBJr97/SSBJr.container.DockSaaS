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

// Add Apache Kafka
var kafka = builder.AddKafka("kafka")
    .WithDataVolume()
    .WithKafkaUI();

// Add API Service - endpoints will be created automatically from launch settings
var apiService = builder.AddProject<Projects.SSBJr_DockSaaS_ApiService>("apiservice")
    .WithReference(docksaasdb)
    .WithReference(redis)
    .WithReference(kafka)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ServiceEndpoints__BaseUrl", "http://apiservice"); // Internal network name

// Add Blazor Web Service - endpoints will be created automatically from launch settings
var webService = builder.AddProject<Projects.SSBJr_DockSaaS_Web>("webservice")
    .WithReference(apiService)
    .WithReference(redis)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ApiBaseUrl", "http://apiservice"); // Internal network name

// Configure service discovery and CORS
apiService.WithEnvironment("BlazorClientUrls", "http://webservice,https://localhost:7001,http://localhost:5201"); 
webService.WithEnvironment("ApiServiceUrl", "http://apiservice,https://localhost:7000,http://localhost:5200");

var app = builder.Build();

app.Run();
