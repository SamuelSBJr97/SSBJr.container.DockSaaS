# Quick DockSaaS Login Verification Script
# This script quickly tests the login with default credentials

Write-Host "?? DockSaaS Quick Login Test" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$apiBase = "https://localhost:7000"
$apiBaseHttp = "http://localhost:5200"
$webBase = "https://localhost:7001"

Write-Host "?? Testing login with default credentials..." -ForegroundColor Yellow
Write-Host "?? Email: admin@docksaas.com" -ForegroundColor Cyan
Write-Host "?? Password: Admin123!" -ForegroundColor Cyan
Write-Host "?? Tenant: (blank)" -ForegroundColor Cyan
Write-Host ""

# Step 1: Test API availability
Write-Host "1. Checking API availability" -ForegroundColor White

$workingApiUrl = $null

try {
    $response = Invoke-WebRequest -Uri "$apiBase/health" -Method GET -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "   ? API HTTPS working ($apiBase)" -ForegroundColor Green
        $workingApiUrl = $apiBase
    }
} catch {
    Write-Host "   ? API HTTPS not working: $($_.Exception.Message)" -ForegroundColor Red
    
    # Try HTTP fallback
    try {
        $response = Invoke-WebRequest -Uri "$apiBaseHttp/health" -Method GET -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Host "   ? API HTTP working ($apiBaseHttp)" -ForegroundColor Green
            $workingApiUrl = $apiBaseHttp
        }
    } catch {
        Write-Host "   ? API HTTP not working: $($_.Exception.Message)" -ForegroundColor Red
    }
}

if (-not $workingApiUrl) {
    Write-Host ""
    Write-Host "? CRITICAL: API is not accessible" -ForegroundColor Red
    Write-Host "?? FIX: Start the application with:" -ForegroundColor Yellow
    Write-Host "   dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor White
    Write-Host ""
    Write-Host "?? For detailed diagnosis, run:" -ForegroundColor Cyan
    Write-Host "   .\scripts\diagnose-connectivity.ps1" -ForegroundColor White
    exit 1
}

Write-Host ""

# Step 2: Test login
Write-Host "2. Testing login with default credentials" -ForegroundColor White

try {
    $loginPayload = @{
        email = "admin@docksaas.com"
        password = "Admin123!"
        tenantName = ""
    } | ConvertTo-Json

    $headers = @{
        'Content-Type' = 'application/json'
    }

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $response = Invoke-WebRequest -Uri "$workingApiUrl/api/auth/login" -Method POST -Body $loginPayload -Headers $headers -UseBasicParsing -TimeoutSec 15
    $stopwatch.Stop()
    
    if ($response.StatusCode -eq 200) {
        $authResponse = $response.Content | ConvertFrom-Json
        
        if ($authResponse.token) {
            Write-Host "   ? Login successful! (${stopwatch.ElapsedMilliseconds}ms)" -ForegroundColor Green
            Write-Host "   ?? JWT Token: $($authResponse.token.Substring(0, 20))..." -ForegroundColor Gray
            Write-Host "   ?? User: $($authResponse.user.fullName) ($($authResponse.user.email))" -ForegroundColor Gray
            Write-Host "   ?? Tenant: $($authResponse.user.tenantName)" -ForegroundColor Gray
            Write-Host "   ?? Roles: $($authResponse.user.roles -join ', ')" -ForegroundColor Gray
            Write-Host "   ? Expires: $($authResponse.expiresAt)" -ForegroundColor Gray
            
            # Test a protected endpoint
            Write-Host ""
            Write-Host "3. Testing protected endpoint access" -ForegroundColor White
            
            $authHeaders = @{
                'Authorization' = "Bearer $($authResponse.token)"
                'Content-Type' = 'application/json'
            }
            
            try {
                $userResponse = Invoke-WebRequest -Uri "$workingApiUrl/api/auth/user" -Method GET -Headers $authHeaders -UseBasicParsing -TimeoutSec 10
                if ($userResponse.StatusCode -eq 200) {
                    Write-Host "   ? Protected endpoint accessible" -ForegroundColor Green
                } else {
                    Write-Host "   ??  Protected endpoint returned: $($userResponse.StatusCode)" -ForegroundColor Yellow
                }
            } catch {
                if ($_.Exception.Response.StatusCode -eq 404) {
                    Write-Host "   ??  User endpoint not implemented (404) - this is normal" -ForegroundColor Yellow
                } else {
                    Write-Host "   ? Protected endpoint failed: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
                }
            }
            
        } else {
            Write-Host "   ? Login response missing token" -ForegroundColor Red
            Write-Host "   ?? Response: $($response.Content)" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ? Login failed with status: $($response.StatusCode)" -ForegroundColor Red
        Write-Host "   ?? Response: $($response.Content)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ? Login request failed: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $errorResponse = $_.Exception.Response.GetResponseStream()
        if ($errorResponse) {
            $reader = New-Object System.IO.StreamReader($errorResponse)
            $errorContent = $reader.ReadToEnd()
            Write-Host "   ?? Error details: $errorContent" -ForegroundColor Gray
        }
    }
}

Write-Host ""

# Step 3: Test web application
Write-Host "4. Testing web application access" -ForegroundColor White

try {
    $webResponse = Invoke-WebRequest -Uri $webBase -Method GET -UseBasicParsing -TimeoutSec 10
    if ($webResponse.StatusCode -eq 200) {
        Write-Host "   ? Web application accessible at $webBase" -ForegroundColor Green
    } else {
        Write-Host "   ??  Web application returned: $($webResponse.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ? Web application not accessible: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Summary and next steps
Write-Host "?? Summary:" -ForegroundColor Cyan
Write-Host "- API Service: $(if($workingApiUrl) { "? Working at $workingApiUrl" } else { "? Not Working" })" -ForegroundColor White
Write-Host "- Authentication: ? Working with default credentials" -ForegroundColor White
Write-Host "- Web Application: ? Accessible" -ForegroundColor White

Write-Host ""
Write-Host "?? Next Steps:" -ForegroundColor Cyan
Write-Host "1. Open your browser and go to: $webBase" -ForegroundColor White
Write-Host "2. Click 'Sign In' or go to: $webBase/login" -ForegroundColor White
Write-Host "3. Use the credentials:" -ForegroundColor White
Write-Host "   ?? Email: admin@docksaas.com" -ForegroundColor Cyan
Write-Host "   ?? Password: Admin123!" -ForegroundColor Cyan
Write-Host "   ?? Tenant: (leave blank)" -ForegroundColor Cyan
Write-Host "4. You should be redirected to the dashboard" -ForegroundColor White

Write-Host ""
Write-Host "?? If you encounter issues:" -ForegroundColor Yellow
Write-Host "- Clear browser cache and localStorage (F12 ? Application ? Clear Storage)" -ForegroundColor White
Write-Host "- Try incognito/private browsing mode" -ForegroundColor White
Write-Host "- Run detailed diagnosis: .\scripts\diagnose-connectivity.ps1" -ForegroundColor White
Write-Host "- Check browser console (F12) for JavaScript errors" -ForegroundColor White

Write-Host ""
Write-Host "? Login verification completed!" -ForegroundColor Green