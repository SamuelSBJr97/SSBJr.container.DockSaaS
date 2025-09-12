using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Blazored.LocalStorage;
using SSBJr.DockSaaS.Web.Models;
using Microsoft.JSInterop;

namespace SSBJr.DockSaaS.Web.Services;

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

    private async Task<bool> IsJavaScriptAvailableAsync()
    {
        try
        {
            // Check if JavaScript is available (not during prerendering)
            await _jsRuntime.InvokeVoidAsync("eval", "void(0)");
            return true;
        }
        catch (InvalidOperationException)
        {
            // JavaScript interop is not available during prerendering
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // If JavaScript is not available (prerendering), return anonymous user
            if (!await IsJavaScriptAvailableAsync())
            {
                _logger.LogDebug("JavaScript not available (prerendering), returning anonymous user");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var token = await _localStorage.GetItemAsync<string>("authToken");
            var user = await _localStorage.GetItemAsync<UserDto>("currentUser");

            if (string.IsNullOrEmpty(token) || user == null)
            {
                _logger.LogDebug("No token or user found in localStorage, returning anonymous user");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var expiry = await _localStorage.GetItemAsync<DateTime>("tokenExpiry");
            if (expiry != default && expiry <= DateTime.UtcNow)
            {
                _logger.LogDebug("Token expired, clearing localStorage and returning anonymous user");
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("currentUser");
                await _localStorage.RemoveItemAsync("tokenExpiry");
                await _localStorage.RemoveItemAsync("refreshToken");
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

            _logger.LogDebug("User {Email} authenticated successfully with {RoleCount} roles", user.Email, user.Roles.Count);
            return new AuthenticationState(principal);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            _logger.LogDebug("JavaScript interop not available (prerendering), returning anonymous user");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error getting authentication state, returning anonymous user");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void MarkUserAsAuthenticated(UserDto user)
    {
        try
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

            _logger.LogDebug("Marking user {Email} as authenticated", user.Email);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking user as authenticated");
        }
    }

    public void MarkUserAsLoggedOut()
    {
        try
        {
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            _logger.LogDebug("Marking user as logged out");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking user as logged out");
        }
    }
}