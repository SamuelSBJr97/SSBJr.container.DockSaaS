using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace SSBJr.DockSaaS.Web.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<ApiClient> _logger;
    private readonly IJSRuntime _jsRuntime;

    public ApiClient(HttpClient httpClient, ILocalStorageService localStorage, ILogger<ApiClient> logger, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _logger = logger;
        _jsRuntime = jsRuntime;
        
        // Initialize the authorization header on startup
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(1000); // Wait a moment for the app to fully initialize
                await SetAuthorizationHeaderAsync();
                _logger.LogDebug("ApiClient initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error during ApiClient initialization (expected during prerendering)");
            }
        });
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

    private async Task SetAuthorizationHeaderAsync()
    {
        try
        {
            // Only try to access localStorage if JavaScript is available
            if (!await IsJavaScriptAvailableAsync())
            {
                _logger.LogDebug("JavaScript not available (prerendering), skipping authorization header setup");
                return;
            }

            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                // Verify token hasn't expired
                var expiry = await _localStorage.GetItemAsync<DateTime>("tokenExpiry");
                if (expiry != default && expiry <= DateTime.UtcNow)
                {
                    _logger.LogDebug("Token expired, clearing authorization header");
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    
                    // Clear expired tokens from storage
                    await _localStorage.RemoveItemAsync("authToken");
                    await _localStorage.RemoveItemAsync("tokenExpiry");
                    await _localStorage.RemoveItemAsync("refreshToken");
                    await _localStorage.RemoveItemAsync("currentUser");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Authorization header set with valid token (expires: {Expiry})", expiry);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _logger.LogDebug("No token found, authorization header cleared");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            _logger.LogDebug("JavaScript interop not available (prerendering), skipping authorization header setup");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set authorization header");
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    /// <summary>
    /// Checks if the API health endpoint is accessible
    /// </summary>
    /// <returns>True if the health endpoint responds successfully, false otherwise</returns>
    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            _logger.LogInformation("Checking API health at {BaseAddress}", _httpClient.BaseAddress);
            
            var response = await _httpClient.GetAsync("health");
            
            _logger.LogInformation("Health check returned status {StatusCode}", response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Health check response: {Content}", content.Length > 100 ? content.Substring(0, 100) + "..." : content);
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Health check failed with status {StatusCode}. Error: {Error}", 
                response.StatusCode, errorContent);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during health check. BaseAddress: {BaseAddress}. Message: {Message}", 
                _httpClient.BaseAddress, ex.Message);
            return false;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout during health check");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request cancelled during health check");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during health check");
            return false;
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            _logger.LogInformation("Making GET request to {Endpoint} with base address {BaseAddress}", endpoint, _httpClient.BaseAddress);
            
            var response = await _httpClient.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("GET request to {Endpoint} returned status {StatusCode}", endpoint, response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("GET request to {Endpoint} succeeded. Response length: {Length}", endpoint, content.Length);
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("GET request to {Endpoint} succeeded but returned empty content", endpoint);
                    return default;
                }
                
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            _logger.LogError("GET request to {Endpoint} failed with status {StatusCode}. Error: {Error}", 
                endpoint, response.StatusCode, content);
            return default;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error making GET request to {Endpoint}. BaseAddress: {BaseAddress}. Message: {Message}", 
                endpoint, _httpClient.BaseAddress, ex.Message);
            return default;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout making GET request to {Endpoint}", endpoint);
            return default;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request cancelled making GET request to {Endpoint}", endpoint);
            return default;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for GET request to {Endpoint}", endpoint);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error making GET request to {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Making POST request to {Endpoint} with base address {BaseAddress}", endpoint, _httpClient.BaseAddress);
            _logger.LogDebug("POST request payload: {Payload}", json.Length > 500 ? json.Substring(0, 500) + "..." : json);
            
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("POST request to {Endpoint} returned status {StatusCode}", endpoint, response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("POST request to {Endpoint} succeeded. Response length: {Length}", endpoint, responseContent.Length);
                
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    _logger.LogWarning("POST request to {Endpoint} succeeded but returned empty content", endpoint);
                    return default;
                }
                
                return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            _logger.LogError("POST request to {Endpoint} failed with status {StatusCode}. Error: {Error}", 
                endpoint, response.StatusCode, responseContent);
            return default;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error making POST request to {Endpoint}. BaseAddress: {BaseAddress}. Message: {Message}", 
                endpoint, _httpClient.BaseAddress, ex.Message);
            return default;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout making POST request to {Endpoint}", endpoint);
            return default;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request cancelled making POST request to {Endpoint}", endpoint);
            return default;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization/deserialization error for POST request to {Endpoint}", endpoint);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error making POST request to {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest request)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Making POST request to {Endpoint}", endpoint);
            
            var response = await _httpClient.PostAsync(endpoint, content);
            
            _logger.LogInformation("POST request to {Endpoint} returned status {StatusCode}", endpoint, response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("POST request to {Endpoint} failed with status {StatusCode}. Error: {Error}", 
                    endpoint, response.StatusCode, errorContent);
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making POST request to {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<bool> PostAsync(string endpoint)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            
            _logger.LogInformation("Making POST request to {Endpoint}", endpoint);
            
            var response = await _httpClient.PostAsync(endpoint, null);
            
            _logger.LogInformation("POST request to {Endpoint} returned status {StatusCode}", endpoint, response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("POST request to {Endpoint} failed with status {StatusCode}. Error: {Error}", 
                    endpoint, response.StatusCode, errorContent);
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making POST request to {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("PUT request to {Endpoint} failed with status {StatusCode}. Error: {Error}", 
                endpoint, response.StatusCode, errorContent);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making PUT request to {Endpoint}", endpoint);
            return default;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("DELETE request to {Endpoint} failed with status {StatusCode}. Error: {Error}", 
                    endpoint, response.StatusCode, errorContent);
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making DELETE request to {Endpoint}", endpoint);
            return false;
        }
    }

    /// <summary>
    /// Manually sets the authorization token for scenarios where localStorage is not available
    /// </summary>
    public void SetAuthorizationToken(string? token)
    {
        try
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Authorization token manually set");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _logger.LogDebug("Authorization token manually cleared");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting authorization token manually");
        }
    }

    /// <summary>
    /// Force refresh authorization header from localStorage
    /// </summary>
    public async Task RefreshAuthorizationAsync()
    {
        await SetAuthorizationHeaderAsync();
    }
}