using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using SSBJr.DockSaaS.Web.Models;
using System.Text.Json;

namespace SSBJr.DockSaaS.Web.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> RefreshTokenAsync();
}

public class AuthService : IAuthService
{
    private readonly ApiClient _apiClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApiClient apiClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider,
        ILogger<AuthService> logger)
    {
        _apiClient = apiClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {Email}", request.Email);

            var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>("api/auth/login", request);
            
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                _logger.LogInformation("Login successful for user: {Email}", request.Email);
                
                // Store authentication data
                await _localStorage.SetItemAsync("authToken", response.Token);
                await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
                await _localStorage.SetItemAsync("tokenExpiry", response.ExpiresAt);
                await _localStorage.SetItemAsync("currentUser", response.User);

                // Update API client with new token
                _apiClient.SetAuthorizationToken(response.Token);

                _logger.LogInformation("Authentication data stored successfully for user: {Email}", request.Email);
                return true;
            }
            else
            {
                _logger.LogWarning("Login failed for user: {Email}. Response: {Response}", 
                    request.Email, JsonSerializer.Serialize(response));
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for user: {Email}", request.Email);
            return false;
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting registration for user: {Email}", request.Email);

            var response = await _apiClient.PostAsync<RegisterRequest, LoginResponse>("api/auth/register", request);
            
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                _logger.LogInformation("Registration successful for user: {Email}", request.Email);
                
                // Store authentication data
                await _localStorage.SetItemAsync("authToken", response.Token);
                await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
                await _localStorage.SetItemAsync("tokenExpiry", response.ExpiresAt);
                await _localStorage.SetItemAsync("currentUser", response.User);

                // Update API client with new token
                _apiClient.SetAuthorizationToken(response.Token);

                _logger.LogInformation("Authentication data stored successfully for new user: {Email}", request.Email);
                return true;
            }
            else
            {
                _logger.LogWarning("Registration failed for user: {Email}. Response: {Response}", 
                    request.Email, JsonSerializer.Serialize(response));
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error for user: {Email}", request.Email);
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            _logger.LogInformation("Logging out current user");

            // Call logout endpoint if available
            try
            {
                await _apiClient.PostAsync("api/auth/logout");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calling logout endpoint (non-critical)");
            }

            // Clear local storage
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            await _localStorage.RemoveItemAsync("tokenExpiry");
            await _localStorage.RemoveItemAsync("currentUser");

            // Clear API client token
            _apiClient.SetAuthorizationToken(null);

            // Update authentication state
            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.MarkUserAsLoggedOut();
            }

            _logger.LogInformation("User logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            throw;
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            var user = await _localStorage.GetItemAsync<UserDto>("currentUser");
            if (user != null)
            {
                _logger.LogDebug("Retrieved current user from localStorage: {Email}", user.Email);
                return user;
            }

            _logger.LogDebug("No current user found in localStorage");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            var expiry = await _localStorage.GetItemAsync<DateTime>("tokenExpiry");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("No authentication token found");
                return false;
            }

            if (expiry != default && expiry <= DateTime.UtcNow)
            {
                _logger.LogDebug("Authentication token expired");
                await LogoutAsync(); // Clear expired token
                return false;
            }

            _logger.LogDebug("User is authenticated with valid token");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            return false;
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to refresh authentication token");

            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("No refresh token available");
                return false;
            }

            var request = new { RefreshToken = refreshToken };
            var response = await _apiClient.PostAsync<object, LoginResponse>("api/auth/refresh", request);
            
            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                _logger.LogInformation("Token refresh successful");
                
                // Update stored authentication data
                await _localStorage.SetItemAsync("authToken", response.Token);
                await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
                await _localStorage.SetItemAsync("tokenExpiry", response.ExpiresAt);
                await _localStorage.SetItemAsync("currentUser", response.User);

                // Update API client with new token
                _apiClient.SetAuthorizationToken(response.Token);

                // Refresh authentication state
                if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
                {
                    await customProvider.RefreshAuthenticationStateAsync();
                }

                return true;
            }
            else
            {
                _logger.LogWarning("Token refresh failed: {Response}", JsonSerializer.Serialize(response));
                await LogoutAsync(); // Clear invalid tokens
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            await LogoutAsync(); // Clear potentially invalid tokens
            return false;
        }
    }
}