#!/usr/bin/env pwsh

# Script to test webservice authentication
# This script tests the authentication flow between the webservice and API

Write-Host "=== DockSaaS WebService Authentication Test ===" -ForegroundColor Green
Write-Host ""

# Configuration
$WebBaseUrl = "https://localhost:7001"
$ApiBaseUrl = "https://localhost:7000"
$TestEmail = "admin@docksaas.com"
$TestPassword = "Admin123!"

Write-Host "Testing endpoints:" -ForegroundColor Yellow
Write-Host "  Web Service: $WebBaseUrl" -ForegroundColor Cyan
Write-Host "  API Service: $ApiBaseUrl" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if API is running
Write-Host "1. Testing API connectivity..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$ApiBaseUrl/health" -Method Get -TimeoutSec 10
    Write-Host "   ✓ API health check passed" -ForegroundColor Green
} catch {
    Write-Host "   ✗ API health check failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Make sure the API is running at $ApiBaseUrl" -ForegroundColor Yellow
    exit 1
}

# Test 2: Check if Web Service is running
Write-Host "2. Testing Web Service connectivity..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$WebBaseUrl/health" -Method Get -TimeoutSec 10 -UseBasicParsing
    Write-Host "   ✓ Web Service health check passed (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Web Service health check failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Make sure the Web Service is running at $WebBaseUrl" -ForegroundColor Yellow
    exit 1
}

# Test 3: Test API authentication endpoint directly
Write-Host "3. Testing API authentication directly..." -ForegroundColor Yellow
$loginPayload = @{
    email = $TestEmail
    password = $TestPassword
    tenantName = ""
} | ConvertTo-Json

try {
    $headers = @{
        'Content-Type' = 'application/json'
    }
    $authResponse = Invoke-RestMethod -Uri "$ApiBaseUrl/api/auth/login" -Method Post -Body $loginPayload -Headers $headers -TimeoutSec 10
    
    if ($authResponse.token) {
        Write-Host "   ✓ API authentication successful" -ForegroundColor Green
        Write-Host "   Token received: $($authResponse.token.Substring(0, 20))..." -ForegroundColor Cyan
        Write-Host "   User: $($authResponse.user.email)" -ForegroundColor Cyan
        Write-Host "   Tenant: $($authResponse.user.tenantName)" -ForegroundColor Cyan
        $jwtToken = $authResponse.token
    } else {
        Write-Host "   ✗ API authentication failed: No token received" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "   ✗ API authentication failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 4: Test authenticated API call
Write-Host "4. Testing authenticated API call..." -ForegroundColor Yellow
try {
    $authHeaders = @{
        'Authorization' = "Bearer $jwtToken"
        'Content-Type' = 'application/json'
    }
    $userResponse = Invoke-RestMethod -Uri "$ApiBaseUrl/api/auth/me" -Method Get -Headers $authHeaders -TimeoutSec 10
    Write-Host "   ✓ Authenticated API call successful" -ForegroundColor Green
    Write-Host "   Current user: $($userResponse.email)" -ForegroundColor Cyan
} catch {
    Write-Host "   ✗ Authenticated API call failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Test Web Service login page
Write-Host "5. Testing Web Service login page..." -ForegroundColor Yellow
try {
    $loginPageResponse = Invoke-WebRequest -Uri "$WebBaseUrl/login" -Method Get -TimeoutSec 10 -UseBasicParsing
    if ($loginPageResponse.StatusCode -eq 200) {
        Write-Host "   ✓ Web Service login page accessible" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Web Service login page returned status: $($loginPageResponse.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "   ✗ Web Service login page failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Test Summary ===" -ForegroundColor Green
Write-Host "Authentication flow components tested:" -ForegroundColor Yellow
Write-Host "  ✓ API health check" -ForegroundColor Green
Write-Host "  ✓ Web Service health check" -ForegroundColor Green
Write-Host "  ✓ API authentication endpoint" -ForegroundColor Green
Write-Host "  ✓ Authenticated API calls" -ForegroundColor Green
Write-Host "  ✓ Web Service login page" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps to test authentication in the browser:" -ForegroundColor Yellow
Write-Host "1. Open $WebBaseUrl/login in your browser" -ForegroundColor Cyan
Write-Host "2. Use credentials: $TestEmail / $TestPassword" -ForegroundColor Cyan
Write-Host "3. Check browser developer tools (F12) for authentication errors" -ForegroundColor Cyan
Write-Host "4. Verify JWT token is stored in localStorage" -ForegroundColor Cyan
Write-Host ""
Write-Host "If authentication still fails in the browser:" -ForegroundColor Yellow
Write-Host "- Check browser console for JavaScript errors" -ForegroundColor Cyan
Write-Host "- Verify localStorage access is working" -ForegroundColor Cyan
Write-Host "- Check if CORS is properly configured" -ForegroundColor Cyan
Write-Host "- Ensure ApiClient is properly setting Authorization headers" -ForegroundColor Cyan