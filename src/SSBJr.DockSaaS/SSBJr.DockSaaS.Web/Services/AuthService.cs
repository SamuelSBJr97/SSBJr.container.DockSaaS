using SSBJr.DockSaaS.Web.Models;
using Blazored.LocalStorage;

namespace SSBJr.DockSaaS.Web.Services;

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

    public AuthService(ApiClient apiClient, ILocalStorageService localStorage, ILogger<AuthService> logger)
    {
        _apiClient = apiClient;
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>("api/auth/login", request);
            
            if (response != null)
            {
                await _localStorage.SetItemAsync("authToken", response.Token);
                await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
                await _localStorage.SetItemAsync("tokenExpiry", response.ExpiresAt);
                await _localStorage.SetItemAsync("currentUser", response.User);
                
                _logger.LogInformation("User {Email} logged in successfully", request.Email);
                return true;
            }
            
            return false;
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
            
            if (response != null)
            {
                await _localStorage.SetItemAsync("authToken", response.Token);
                await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
                await _localStorage.SetItemAsync("tokenExpiry", response.ExpiresAt);
                await _localStorage.SetItemAsync("currentUser", response.User);
                
                _logger.LogInformation("User {Email} registered and logged in successfully", request.Email);
                return true;
            }
            
            return false;
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
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            await _localStorage.RemoveItemAsync("tokenExpiry");
            await _localStorage.RemoveItemAsync("currentUser");

            // Always try to call the API logout
            try
            {
                await _apiClient.PostAsync("api/auth/logout");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "API logout call failed, but local storage cleared");
            }
            
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
            // Try to get from local storage first
            var cachedUser = await _localStorage.GetItemAsync<UserDto>("currentUser");
            if (cachedUser != null)
                return cachedUser;

            // If not in cache, try to get from API
            var user = await _apiClient.GetAsync<UserDto>("api/auth/me");
            if (user != null)
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
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrEmpty(token))
                return false;

            var expiry = await _localStorage.GetItemAsync<DateTime>("tokenExpiry");
            if (expiry != default && expiry <= DateTime.UtcNow)
            {
                await LogoutAsync();
                return false;
            }

            return !string.IsNullOrEmpty(token);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to check authentication status, assuming not authenticated");
            return false;
        }
    }
}