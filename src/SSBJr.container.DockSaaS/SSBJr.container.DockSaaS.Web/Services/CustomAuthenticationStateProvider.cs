using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Blazored.LocalStorage;
using SSBJr.container.DockSaaS.Web.Models;
using Microsoft.JSInterop;

namespace SSBJr.container.DockSaaS.Web.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private readonly IJSRuntime _jsRuntime;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage, ILogger<CustomAuthenticationStateProvider> logger, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _logger = logger;
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // During prerendering, JavaScript interop is not available
            if (!IsJavaScriptRuntimeAvailable())
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var token = await _localStorage.GetItemAsync<string>("authToken");
            var user = await _localStorage.GetItemAsync<UserDto>("currentUser");

            if (string.IsNullOrEmpty(token) || user == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var expiry = await _localStorage.GetItemAsync<DateTime>("tokenExpiry");
            if (expiry <= DateTime.UtcNow)
            {
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("currentUser");
                await _localStorage.RemoveItemAsync("tokenExpiry");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new("tenant_id", user.TenantId.ToString()),
                new("tenant_name", user.TenantName),
                new("first_name", user.FirstName),
                new("last_name", user.LastName)
            };

            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);

            return new AuthenticationState(principal);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            // This happens during prerendering - return anonymous user
            _logger.LogDebug("JavaScript interop not available during prerendering");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication state");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void MarkUserAsAuthenticated(UserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("tenant_id", user.TenantId.ToString()),
            new("tenant_name", user.TenantName),
            new("first_name", user.FirstName),
            new("last_name", user.LastName)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public void MarkUserAsLoggedOut()
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private bool IsJavaScriptRuntimeAvailable()
    {
        // Check if we're in a prerendering context
        return _jsRuntime is not IJSInProcessRuntime && 
               !(_jsRuntime.GetType().Name.Contains("UnsupportedJavaScriptRuntime"));
    }
}