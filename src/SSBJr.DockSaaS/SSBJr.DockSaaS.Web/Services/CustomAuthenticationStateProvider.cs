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
    private AuthenticationState _currentAuthState;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage, ILogger<CustomAuthenticationStateProvider> logger, IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _logger = logger;
        _jsRuntime = jsRuntime;
        _currentAuthState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
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
            // If JavaScript is not available (prerendering), return cached state or anonymous user
            if (!await IsJavaScriptAvailableAsync())
            {
                _logger.LogDebug("JavaScript not available (prerendering), returning cached authentication state");
                return _currentAuthState;
            }

            var token = await _localStorage.GetItemAsync<string>("authToken");
            var user = await _localStorage.GetItemAsync<UserDto>("currentUser");

            if (string.IsNullOrEmpty(token) || user == null)
            {
                _logger.LogDebug("No token or user found in localStorage, returning anonymous user");
                _currentAuthState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                return _currentAuthState;
            }

            var expiry = await _localStorage.GetItemAsync<DateTime>("tokenExpiry");
            if (expiry != default && expiry <= DateTime.UtcNow)
            {
                _logger.LogDebug("Token expired, clearing localStorage and returning anonymous user");
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("currentUser");
                await _localStorage.RemoveItemAsync("tokenExpiry");
                await _localStorage.RemoveItemAsync("refreshToken");
                
                _currentAuthState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                return _currentAuthState;
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
            _currentAuthState = new AuthenticationState(principal);
            return _currentAuthState;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            _logger.LogDebug("JavaScript interop not available (prerendering), returning cached authentication state");
            return _currentAuthState;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error getting authentication state, returning anonymous user");
            _currentAuthState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            return _currentAuthState;
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

            _currentAuthState = new AuthenticationState(principal);
            
            _logger.LogInformation("Marking user {Email} as authenticated and notifying state change", user.Email);
            NotifyAuthenticationStateChanged(Task.FromResult(_currentAuthState));
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

            _currentAuthState = new AuthenticationState(principal);
            
            _logger.LogInformation("Marking user as logged out and notifying state change");
            NotifyAuthenticationStateChanged(Task.FromResult(_currentAuthState));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking user as logged out");
        }
    }

    /// <summary>
    /// Force refresh of authentication state from storage
    /// </summary>
    public async Task RefreshAuthenticationStateAsync()
    {
        try
        {
            _logger.LogInformation("Refreshing authentication state");
            var newState = await GetAuthenticationStateAsync();
            _currentAuthState = newState;
            NotifyAuthenticationStateChanged(Task.FromResult(newState));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing authentication state");
        }
    }
}