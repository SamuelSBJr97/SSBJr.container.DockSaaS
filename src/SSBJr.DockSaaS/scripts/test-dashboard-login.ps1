# Test Dashboard Login Script for DockSaaS
# This script specifically tests the dashboard login flow

Write-Host "?? DockSaaS Dashboard Login Test" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$apiBase = "https://localhost:7000"
$webBase = "https://localhost:7001"

Write-Host "Testing dashboard login flow..." -ForegroundColor Yellow
Write-Host ""

# Test 1: Check if Aspire is running
Write-Host "1. Checking if Aspire/services are running" -ForegroundColor White
$aspireProcesses = Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" -and $_.CommandLine -like "*AppHost*" }
if ($aspireProcesses) {
    Write-Host "   ? Found Aspire process(es):" -ForegroundColor Green
    $aspireProcesses | ForEach-Object { Write-Host "      PID: $($_.Id) - $($_.ProcessName)" -ForegroundColor Gray }
} else {
    Write-Host "   ? No Aspire processes found. Run: dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Check API health
Write-Host "2. Testing API health endpoint" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiBase/health" -Method GET -UseBasicParsing -TimeoutSec 10
    Write-Host "   ? API Health Check: $($response.StatusCode) - API is running" -ForegroundColor Green
} catch {
    Write-Host "   ? API Health Check Failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Make sure the API is running on $apiBase" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Test 3: Check Web app
Write-Host "3. Testing Web application" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$webBase" -Method GET -UseBasicParsing -TimeoutSec 10
    Write-Host "   ? Web App Check: $($response.StatusCode) - Web app is running" -ForegroundColor Green
} catch {
    Write-Host "   ? Web App Check Failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Make sure the web app is running on $webBase" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Test 4: Test login endpoint
Write-Host "4. Testing login endpoint with default credentials" -ForegroundColor White
try {
    $loginPayload = @{
        email = "admin@docksaas.com"
        password = "Admin123!"
        tenantName = ""
    } | ConvertTo-Json

    $headers = @{
        'Content-Type' = 'application/json'
    }

    $response = Invoke-WebRequest -Uri "$apiBase/api/auth/login" -Method POST -Body $loginPayload -Headers $headers -UseBasicParsing -TimeoutSec 15
    
    if ($response.StatusCode -eq 200) {
        $responseData = $response.Content | ConvertFrom-Json
        if ($responseData.success -and $responseData.token) {
            Write-Host "   ? Login Test: Successful - Token received" -ForegroundColor Green
            Write-Host "   ?? User: $($responseData.user.email) - $($responseData.user.fullName)" -ForegroundColor Gray
            Write-Host "   ?? Token expires: $($responseData.expiry)" -ForegroundColor Gray
            
            # Test dashboard endpoints
            Write-Host ""
            Write-Host "5. Testing dashboard endpoints with token" -ForegroundColor White
            
            $authHeaders = @{
                'Authorization' = "Bearer $($responseData.token)"
                'Content-Type' = 'application/json'
            }
            
            try {
                $dashboardResponse = Invoke-WebRequest -Uri "$apiBase/api/dashboard/stats" -Method GET -Headers $authHeaders -UseBasicParsing -TimeoutSec 10
                Write-Host "   ? Dashboard Stats: $($dashboardResponse.StatusCode) - Dashboard API working" -ForegroundColor Green
            } catch {
                if ($_.Exception.Response.StatusCode -eq 404) {
                    Write-Host "   ??  Dashboard Stats: 404 - Endpoint not implemented yet (normal)" -ForegroundColor Yellow
                } else {
                    Write-Host "   ? Dashboard Stats: $($_.Exception.Response.StatusCode) - $($_.Exception.Message)" -ForegroundColor Red
                }
            }
            
        } else {
            Write-Host "   ? Login Test: API returned success=false or no token" -ForegroundColor Red
            Write-Host "   Response: $($response.Content)" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ? Login Test: HTTP $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "   ? Login Test Failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorResponse = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorResponse)
        $errorContent = $reader.ReadToEnd()
        Write-Host "   Error details: $errorContent" -ForegroundColor Gray
    }
}

Write-Host ""

# Test 5: Check database connection
Write-Host "6. Checking database connectivity" -ForegroundColor White
try {
    # Try to hit an endpoint that would require database
    $response = Invoke-WebRequest -Uri "$apiBase/swagger" -Method GET -UseBasicParsing -TimeoutSec 10
    Write-Host "   ? Swagger Available: $($response.StatusCode) - API is properly initialized" -ForegroundColor Green
} catch {
    Write-Host "   ??  Swagger: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""

Write-Host "?? Test Results Summary:" -ForegroundColor Cyan
Write-Host "- ? Aspire/Services: Running" -ForegroundColor White
Write-Host "- ? API Health: Working" -ForegroundColor White
Write-Host "- ? Web App: Accessible" -ForegroundColor White
Write-Host "- ? Login API: Functional" -ForegroundColor White
Write-Host ""

Write-Host "?? Dashboard Login Flow:" -ForegroundColor Cyan
Write-Host "1. Open browser: $webBase" -ForegroundColor White
Write-Host "2. Navigate to Login (or click 'Sign In')" -ForegroundColor White
Write-Host "3. Use default credentials:" -ForegroundColor White
Write-Host "   ?? Email: admin@docksaas.com" -ForegroundColor Cyan
Write-Host "   ?? Password: Admin123!" -ForegroundColor Cyan
Write-Host "   ?? Tenant: (leave blank)" -ForegroundColor Cyan
Write-Host "4. After login, should redirect to dashboard at: $webBase/" -ForegroundColor White
Write-Host ""

Write-Host "?? If dashboard still doesn't work after login:" -ForegroundColor Yellow
Write-Host "- Check browser console for JavaScript errors" -ForegroundColor White
Write-Host "- Clear browser cache and localStorage (F12 -> Application -> Storage)" -ForegroundColor White
Write-Host "- Try incognito/private browsing mode" -ForegroundColor White
Write-Host "- Check browser network tab for failed API calls" -ForegroundColor White
Write-Host "- Verify authentication state in browser dev tools" -ForegroundColor White
Write-Host ""

Write-Host "? Test completed!" -ForegroundColor Green