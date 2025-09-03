using SSBJr.container.DockSaaS.Web.Models;
using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace SSBJr.container.DockSaaS.Web.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<bool> LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
}

public class AuthService : IAuthService
{
    private readonly ApiClient _apiClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<AuthService> _logger;
    private readonly IJSRuntime _jsRuntime;

    public AuthService(ApiClient apiClient, ILocalStorageService localStorage, ILogger<AuthService> logger, IJSRuntime jsRuntime)
    {
        _apiClient = apiClient;
        _localStorage = localStorage;
        _logger = logger;
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>("api/auth/login", request);
            
            if (response != null && IsJavaScriptRuntimeAvailable())
            {
                await _localStorage.SetItemAsync("authToken", response.Token);
                await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
                await _localStorage.SetItemAsync("tokenExpiry", response.ExpiresAt);
                await _localStorage.SetItemAsync("currentUser", response.User);
                
                _logger.LogInformation("User {Email} logged in successfully", request.Email);
                return true;
            }
            
            return response != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user {Email}", request.Email);
            return false;
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _apiClient.PostAsync<RegisterRequest, LoginResponse>("api/auth/register", request);
            
            if (response != null && IsJavaScriptRuntimeAvailable())
            {
                await _localStorage.SetItemAsync("authToken", response.Token);
                await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
                await _localStorage.SetItemAsync("tokenExpiry", response.ExpiresAt);
                await _localStorage.SetItemAsync("currentUser", response.User);
                
                _logger.LogInformation("User {Email} registered and logged in successfully", request.Email);
                return true;
            }
            
            return response != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for user {Email}", request.Email);
            return false;
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            if (IsJavaScriptRuntimeAvailable())
            {
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("refreshToken");
                await _localStorage.RemoveItemAsync("tokenExpiry");
                await _localStorage.RemoveItemAsync("currentUser");
            }

            // Always try to call the API logout even if localStorage is not available
            await _apiClient.PostAsync("api/auth/logout");
            
            _logger.LogInformation("User logged out successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return false;
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            // Only try localStorage if JavaScript runtime is available
            if (IsJavaScriptRuntimeAvailable())
            {
                var cachedUser = await _localStorage.GetItemAsync<UserDto>("currentUser");
                if (cachedUser != null)
                    return cachedUser;
            }

            // If not in cache or localStorage not available, try to get from API
            var user = await _apiClient.GetAsync<UserDto>("api/auth/me");
            if (user != null && IsJavaScriptRuntimeAvailable())
            {
                await _localStorage.SetItemAsync("currentUser", user);
            }
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current user");
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            // During prerendering, JavaScript interop is not available
            if (!IsJavaScriptRuntimeAvailable())
            {
                // During prerendering, assume not authenticated
                // The actual authentication state will be determined after the component renders
                return false;
            }

            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrEmpty(token))
                return false;

            var expiry = await _localStorage.GetItemAsync<DateTime>("tokenExpiry");
            if (expiry <= DateTime.UtcNow)
            {
                await LogoutAsync();
                return false;
            }

            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            // This happens during prerendering - return false for now
            _logger.LogDebug("JavaScript interop not available during prerendering");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check authentication status");
            return false;
        }
    }

    private bool IsJavaScriptRuntimeAvailable()
    {
        // Check if we're in a prerendering context
        return _jsRuntime is not IJSInProcessRuntime && 
               !(_jsRuntime.GetType().Name.Contains("UnsupportedJavaScriptRuntime"));
    }
}