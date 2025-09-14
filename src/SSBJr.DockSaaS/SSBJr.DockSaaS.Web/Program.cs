using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using SSBJr.DockSaaS.Web.Services;
using SSBJr.DockSaaS.Web.Components;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure authentication
builder.Services.AddCascadingAuthenticationState();

// Use JWT authentication instead of cookies for API consistency
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // JWT options will be configured by the CustomAuthenticationStateProvider
        options.RequireHttpsMetadata = false; // Allow HTTP in development
        options.SaveToken = true;
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // For Blazor Server, we'll handle JWT via custom provider
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                context.HttpContext.RequestServices.GetService<ILogger<Program>>()?
                    .LogDebug("JWT authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IServiceInstanceService, ServiceInstanceService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Configure HTTP client for API communication with Aspire service discovery
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://apiservice";
var fallbackApiUrl = "https://localhost:7000";

// Add HTTP client with Aspire service discovery
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "DockSaaS-Web/1.0");
});

// Add local storage
builder.Services.AddBlazoredLocalStorage(config =>
{
    config.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
    config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    config.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
    config.JsonSerializerOptions.WriteIndented = false;
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("web_service", () => HealthCheckResult.Healthy("Web service is running"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add health checks endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Log startup information
app.Logger.LogInformation("DockSaaS Web application started");
app.Logger.LogInformation("API Base URL configured as: {ApiBaseUrl}", apiBaseUrl);
app.Logger.LogInformation("Fallback API URL: {FallbackApiUrl}", fallbackApiUrl);
app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
