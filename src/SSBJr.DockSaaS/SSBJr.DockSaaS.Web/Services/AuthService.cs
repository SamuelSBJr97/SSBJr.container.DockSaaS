using SSBJr.DockSaaS.Web.Models;
using Blazored.LocalStorage;
using Microsoft.JSInterop;

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
    private readonly IJSRuntime _jsRuntime;

    public AuthService(ApiClient apiClient, ILocalStorageService localStorage, ILogger<AuthService> logger, IJSRuntime jsRuntime)
    {
        _apiClient = apiClient;
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

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting login for user {Email} with tenant {TenantName}", 
                request.Email, request.TenantName ?? "default");
            
            // Only try to access localStorage if JavaScript is available
            if (await IsJavaScriptAvailableAsync())
            {
                // Clear any existing tokens before login
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("refreshToken");
                await _localStorage.RemoveItemAsync("tokenExpiry");
                await _localStorage.RemoveItemAsync("currentUser");
            }
            
            var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>("api/auth/login", request);
            
            if (response != null)
            {
                _logger.LogInformation("Login successful for user {Email}. Token expires at {ExpiresAt}", 
                    request.Email, response.ExpiresAt);
                
                // Only store in localStorage if JavaScript is available
                if (await IsJavaScriptAvailableAsync())
                {
                    await _localStorage.SetItemAsync("authToken", response.Token);
                    await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
                    await _localStorage.SetItemAsync("tokenExpiry", response.ExpiresAt);
                    await _localStorage.SetItemAsync("currentUser", response.User);
                    
                    _logger.LogDebug("User data stored in local storage for {Email}. User ID: {UserId}, Tenant: {TenantName}", 
                        request.Email, response.User.Id, response.User.TenantName);
                }
                else
                {
                    // Set the authorization token directly on the API client
                    _apiClient.SetAuthorizationToken(response.Token);
                    _logger.LogDebug("Authorization token set directly on API client (prerendering mode)");
                }
                
                return true;
            }
            
            _logger.LogWarning("Login failed for user {Email} - API returned null response", request.Email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user {Email} with exception: {Message}", request.Email, ex.Message);
            return false;
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting registration for user {Email} with tenant {TenantName}", 
                request.Email, request.TenantName);
            
            // Only try to access localStorage if JavaScript is available
            if (await IsJavaScriptAvailableAsync())
            {
                // Clear any existing tokens before registration
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("refreshToken");
                await _localStorage.RemoveItemAsync("tokenExpiry");
                await _localStorage.RemoveItemAsync("currentUser");
            }
            
            var response = await _apiClient.PostAsync<RegisterRequest, LoginResponse>("api/auth/register", request);
            
            if (response != null)
            {
                _logger.LogInformation("Registration successful for user {Email}. Token expires at {ExpiresAt}", 
                    request.Email, response.ExpiresAt);
                
                // Only store in localStorage if JavaScript is available
                if (await IsJavaScriptAvailableAsync())
                {
                    await _localStorage.SetItemAsync("authToken", response.Token);
                    await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
                    await _localStorage.SetItemAsync("tokenExpiry", response.ExpiresAt);
                    await _localStorage.SetItemAsync("currentUser", response.User);
                    
                    _logger.LogDebug("User data stored in local storage for {Email}. User ID: {UserId}, Tenant: {TenantName}", 
                        request.Email, response.User.Id, response.User.TenantName);
                }
                else
                {
                    // Set the authorization token directly on the API client
                    _apiClient.SetAuthorizationToken(response.Token);
                    _logger.LogDebug("Authorization token set directly on API client (prerendering mode)");
                }
                
                return true;
            }
            
            _logger.LogWarning("Registration failed for user {Email} - API returned null response", request.Email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for user {Email} with exception: {Message}", request.Email, ex.Message);
            return false;
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            var userEmail = "unknown";
            
            // Only try to access localStorage if JavaScript is available
            if (await IsJavaScriptAvailableAsync())
            {
                try
                {
                    var currentUser = await _localStorage.GetItemAsync<UserDto>("currentUser");
                    userEmail = currentUser?.Email ?? "unknown";
                }
                catch { }

                _logger.LogInformation("Attempting logout for user {Email}", userEmail);

                // Clear local storage
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("refreshToken");
                await _localStorage.RemoveItemAsync("tokenExpiry");
                await _localStorage.RemoveItemAsync("currentUser");
                
                _logger.LogDebug("Local storage cleared for user {Email}", userEmail);
            }
            else
            {
                _logger.LogInformation("Attempting logout (prerendering mode)");
            }

            // Clear authorization header
            _apiClient.SetAuthorizationToken(null);

            // Try to call the API logout (don't fail if this doesn't work)
            try
            {
                var apiResult = await _apiClient.PostAsync("api/auth/logout");
                if (apiResult)
                {
                    _logger.LogDebug("API logout call succeeded for user {Email}", userEmail);
                }
                else
                {
                    _logger.LogWarning("API logout call failed for user {Email}", userEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "API logout call failed for user {Email}, but local data cleared", userEmail);
            }
            
            _logger.LogInformation("User {Email} logged out successfully", userEmail);
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
            // Only try to access localStorage if JavaScript is available
            if (await IsJavaScriptAvailableAsync())
            {
                // Try to get from local storage first
                var cachedUser = await _localStorage.GetItemAsync<UserDto>("currentUser");
                if (cachedUser != null)
                {
                    _logger.LogDebug("Retrieved cached user {Email} from local storage", cachedUser.Email);
                    return cachedUser;
                }
            }

            _logger.LogDebug("No cached user found, attempting to get from API");

            // If not in cache, try to get from API
            var user = await _apiClient.GetAsync<UserDto>("api/auth/me");
            if (user != null)
            {
                // Only cache if JavaScript is available
                if (await IsJavaScriptAvailableAsync())
                {
                    await _localStorage.SetItemAsync("currentUser", user);
                    _logger.LogDebug("Retrieved user {Email} from API and cached", user.Email);
                }
                else
                {
                    _logger.LogDebug("Retrieved user {Email} from API (no caching in prerendering mode)", user.Email);
                }
            }
            else
            {
                _logger.LogDebug("No user returned from API");
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
            // Only try to access localStorage if JavaScript is available
            if (!await IsJavaScriptAvailableAsync())
            {
                _logger.LogDebug("JavaScript not available (prerendering), assuming not authenticated");
                return false;
            }

            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("No auth token found in local storage");
                return false;
            }

            var expiry = await _localStorage.GetItemAsync<DateTime>("tokenExpiry");
            if (expiry != default && expiry <= DateTime.UtcNow)
            {
                _logger.LogDebug("Auth token has expired (expired at {ExpiryTime}), logging out", expiry);
                await LogoutAsync();
                return false;
            }

            _logger.LogDebug("Auth token is valid and not expired (expires at {ExpiryTime})", expiry);
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            _logger.LogDebug("JavaScript interop not available (prerendering), assuming not authenticated");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to check authentication status, assuming not authenticated");
            return false;
        }
    }
}