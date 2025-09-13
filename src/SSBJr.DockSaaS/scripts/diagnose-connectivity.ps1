# Comprehensive DockSaaS Connectivity Diagnosis & Fix Script
# This script diagnoses and resolves HttpRequestException issues

Write-Host "?? DockSaaS Connectivity Diagnosis & Fix" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$apiBase = "https://localhost:7000"
$apiBaseHttp = "http://localhost:5200"
$webBase = "https://localhost:7001"
$webBaseHttp = "http://localhost:5201"
$aspireBase = "https://localhost:17090"

Write-Host "?? Diagnosing HttpRequestException issues..." -ForegroundColor Yellow
Write-Host ""

# Step 1: Check if processes are running
Write-Host "1. Checking running processes" -ForegroundColor White
$processes = Get-Process | Where-Object { $_.ProcessName -eq "dotnet" }
$aspireProcesses = $processes | Where-Object { $_.MainWindowTitle -like "*AppHost*" -or $_.CommandLine -like "*AppHost*" }
$apiProcesses = $processes | Where-Object { $_.CommandLine -like "*ApiService*" }
$webProcesses = $processes | Where-Object { $_.CommandLine -like "*Web*" -and $_.CommandLine -notlike "*WebView*" }

Write-Host "   ?? Total .NET processes: $($processes.Count)" -ForegroundColor Gray

if ($aspireProcesses) {
    Write-Host "   ? Aspire AppHost is running (PID: $($aspireProcesses[0].Id))" -ForegroundColor Green
} else {
    Write-Host "   ? Aspire AppHost not found" -ForegroundColor Red
}

if ($apiProcesses) {
    Write-Host "   ? API Service is running (PID: $($apiProcesses[0].Id))" -ForegroundColor Green
} else {
    Write-Host "   ? API Service not found" -ForegroundColor Red
}

if ($webProcesses) {
    Write-Host "   ? Web Service is running (PID: $($webProcesses[0].Id))" -ForegroundColor Green
} else {
    Write-Host "   ? Web Service not found" -ForegroundColor Red
}

Write-Host ""

# Step 2: Check port availability and what's using them
Write-Host "2. Checking port availability and usage" -ForegroundColor White
$ports = @(5200, 5201, 7000, 7001, 17090, 9092, 5432, 6379)
$portDescriptions = @{
    5200 = "API HTTP"
    5201 = "Web HTTP"
    7000 = "API HTTPS"
    7001 = "Web HTTPS"
    17090 = "Aspire Dashboard"
    9092 = "Kafka"
    5432 = "PostgreSQL"
    6379 = "Redis"
}

foreach ($port in $ports) {
    try {
        $netstat = netstat -ano | findstr ":$port "
        if ($netstat) {
            $pid = ($netstat -split '\s+')[-1]
            $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
            $processName = if ($process) { $process.ProcessName } else { "Unknown" }
            Write-Host "   ? Port $port ($($portDescriptions[$port])): Used by $processName (PID: $pid)" -ForegroundColor Green
        } else {
            Write-Host "   ? Port $port ($($portDescriptions[$port])): Not in use" -ForegroundColor Red
        }
    } catch {
        Write-Host "   ??  Port $port ($($portDescriptions[$port])): Cannot check" -ForegroundColor Yellow
    }
}

Write-Host ""

# Step 3: Test HTTP endpoints with detailed error analysis
Write-Host "3. Testing HTTP endpoints with detailed diagnostics" -ForegroundColor White

function Test-Endpoint {
    param([string]$url, [string]$description)
    
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $response = Invoke-WebRequest -Uri $url -Method GET -UseBasicParsing -TimeoutSec 10
        $stopwatch.Stop()
        
        Write-Host "   ? $description ($url): $($response.StatusCode) (${stopwatch.ElapsedMilliseconds}ms)" -ForegroundColor Green
        return $true
    } catch {
        $stopwatch.Stop()
        $errorType = $_.Exception.GetType().Name
        $errorMessage = $_.Exception.Message
        
        if ($errorMessage -like "*SSL*" -or $errorMessage -like "*certificate*") {
            Write-Host "   ?? $description ($url): SSL/Certificate issue - $errorMessage" -ForegroundColor Magenta
        } elseif ($errorMessage -like "*timeout*" -or $errorMessage -like "*timed out*") {
            Write-Host "   ??  $description ($url): Timeout - $errorMessage" -ForegroundColor Yellow
        } elseif ($errorMessage -like "*connection*" -or $errorMessage -like "*refused*") {
            Write-Host "   ?? $description ($url): Connection refused - service may not be running" -ForegroundColor Red
        } elseif ($errorMessage -like "*name resolution*" -or $errorMessage -like "*DNS*") {
            Write-Host "   ?? $description ($url): DNS/Name resolution issue" -ForegroundColor Red
        } else {
            Write-Host "   ? $description ($url): $errorType - $errorMessage" -ForegroundColor Red
        }
        return $false
    }
}

$apiHttpsWorking = Test-Endpoint "$apiBase/health" "API HTTPS Health"
$apiHttpWorking = Test-Endpoint "$apiBaseHttp/health" "API HTTP Health"
$webHttpsWorking = Test-Endpoint $webBase "Web HTTPS"
$webHttpWorking = Test-Endpoint $webBaseHttp "Web HTTP"
$aspireWorking = Test-Endpoint $aspireBase "Aspire Dashboard"

Write-Host ""

# Step 4: Test service discovery and internal communication
Write-Host "4. Testing service discovery and internal communication" -ForegroundColor White

# Check appsettings for correct configuration
$webAppsettings = "SSBJr.DockSaaS.Web\appsettings.Development.json"
if (Test-Path $webAppsettings) {
    try {
        $webConfig = Get-Content $webAppsettings | ConvertFrom-Json
        $configuredApiUrl = $webConfig.ApiBaseUrl
        Write-Host "   ?? Web app configured to use API at: $configuredApiUrl" -ForegroundColor Gray
        
        if ($configuredApiUrl -eq "http://apiservice") {
            Write-Host "   ? Correct internal service name configured" -ForegroundColor Green
        } elseif ($configuredApiUrl -like "https://localhost:*") {
            Write-Host "   ??  Using external URL - this may cause issues in Aspire" -ForegroundColor Yellow
        } else {
            Write-Host "   ? Unexpected API URL configuration" -ForegroundColor Red
        }
    } catch {
        Write-Host "   ? Cannot parse web configuration" -ForegroundColor Red
    }
} else {
    Write-Host "   ? Web appsettings.Development.json not found" -ForegroundColor Red
}

Write-Host ""

# Step 5: Test authentication flow
Write-Host "5. Testing authentication endpoint" -ForegroundColor White

$workingApiUrl = if ($apiHttpsWorking) { $apiBase } elseif ($apiHttpWorking) { $apiBaseHttp } else { $null }

if ($workingApiUrl) {
    try {
        $loginPayload = @{
            email = "admin@docksaas.com"
            password = "Admin123!"
            tenantName = ""
        } | ConvertTo-Json

        $headers = @{
            'Content-Type' = 'application/json'
        }

        $response = Invoke-WebRequest -Uri "$workingApiUrl/api/auth/login" -Method POST -Body $loginPayload -Headers $headers -UseBasicParsing -TimeoutSec 15
        
        if ($response.StatusCode -eq 200) {
            $authResponse = $response.Content | ConvertFrom-Json
            if ($authResponse.token) {
                Write-Host "   ? Authentication working - JWT token generated" -ForegroundColor Green
                
                # Test authenticated endpoint
                $authHeaders = @{
                    'Authorization' = "Bearer $($authResponse.token)"
                    'Content-Type' = 'application/json'
                }
                
                try {
                    $dashboardResponse = Invoke-WebRequest -Uri "$workingApiUrl/api/dashboard/stats" -Method GET -Headers $authHeaders -UseBasicParsing -TimeoutSec 10
                    Write-Host "   ? Authenticated endpoint working: $($dashboardResponse.StatusCode)" -ForegroundColor Green
                } catch {
                    if ($_.Exception.Response.StatusCode -eq 404) {
                        Write-Host "   ??  Dashboard endpoint not implemented (404) - this is normal" -ForegroundColor Yellow
                    } else {
                        Write-Host "   ? Authenticated endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
                    }
                }
            } else {
                Write-Host "   ? Authentication response missing token" -ForegroundColor Red
            }
        }
    } catch {
        Write-Host "   ? Authentication test failed: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "   ? Cannot test authentication - no working API endpoint" -ForegroundColor Red
}

Write-Host ""

# Step 6: Check HTTPS certificates
Write-Host "6. Checking HTTPS certificates" -ForegroundColor White

try {
    $certCheck = Invoke-WebRequest -Uri "https://localhost:7000/health" -Method GET -UseBasicParsing -TimeoutSec 5
    Write-Host "   ? HTTPS certificates are working" -ForegroundColor Green
} catch {
    if ($_.Exception.Message -like "*SSL*" -or $_.Exception.Message -like "*certificate*") {
        Write-Host "   ? HTTPS certificate issues detected" -ForegroundColor Red
        Write-Host "   ?? Fix: Run 'dotnet dev-certs https --trust' as administrator" -ForegroundColor Yellow
    } else {
        Write-Host "   ??  Cannot determine certificate status" -ForegroundColor Yellow
    }
}

Write-Host ""

# Step 7: Check for common issues and provide fixes
Write-Host "?? Diagnosis Results & Fixes:" -ForegroundColor Cyan

$hasErrors = $false

# Check if services are not running
if (-not $aspireProcesses -and -not $apiProcesses -and -not $webProcesses) {
    Write-Host ""
    Write-Host "? CRITICAL: No services are running" -ForegroundColor Red
    Write-Host "   ?? FIX: Start the application with:" -ForegroundColor Yellow
    Write-Host "      dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor White
    $hasErrors = $true
}

# Check API connectivity
if (-not $apiHttpsWorking -and -not $apiHttpWorking) {
    Write-Host ""
    Write-Host "? CRITICAL: API service not accessible" -ForegroundColor Red
    Write-Host "   ?? FIXES:" -ForegroundColor Yellow
    Write-Host "      1. Ensure API service is running" -ForegroundColor White
    Write-Host "      2. Check if port 7000/5200 is being used by another process" -ForegroundColor White
    Write-Host "      3. Restart the Aspire application" -ForegroundColor White
    $hasErrors = $true
}

# Check service discovery configuration
if (Test-Path $webAppsettings) {
    $webConfig = Get-Content $webAppsettings | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($webConfig -and $webConfig.ApiBaseUrl -ne "http://apiservice") {
        Write-Host ""
        Write-Host "??  Service Discovery Configuration Issue" -ForegroundColor Yellow
        Write-Host "   ?? FIX: Update appsettings.Development.json:" -ForegroundColor Yellow
        Write-Host '      "ApiBaseUrl": "http://apiservice"' -ForegroundColor White
    }
}

# Check for certificate issues
if ($apiHttpWorking -and -not $apiHttpsWorking) {
    Write-Host ""
    Write-Host "??  HTTPS Certificate Issue" -ForegroundColor Yellow
    Write-Host "   ?? FIX: Trust development certificates:" -ForegroundColor Yellow
    Write-Host "      dotnet dev-certs https --trust" -ForegroundColor White
}

Write-Host ""

# Step 8: Provide specific solutions for HttpRequestException
Write-Host "?? HttpRequestException Troubleshooting:" -ForegroundColor Cyan
Write-Host ""
Write-Host "Common causes and solutions:" -ForegroundColor White
Write-Host ""

Write-Host "1. ?? Connection Refused:" -ForegroundColor Yellow
Write-Host "   - Ensure API service is running on correct port" -ForegroundColor White
Write-Host "   - Check for port conflicts with other applications" -ForegroundColor White
Write-Host "   - Verify Aspire is managing service discovery correctly" -ForegroundColor White
Write-Host ""

Write-Host "2. ??  Timeout Issues:" -ForegroundColor Yellow
Write-Host "   - Check if services are overwhelmed or starting slowly" -ForegroundColor White
Write-Host "   - Increase timeout values in HttpClient configuration" -ForegroundColor White
Write-Host "   - Check for blocking operations in startup code" -ForegroundColor White
Write-Host ""

Write-Host "3. ?? SSL/Certificate Issues:" -ForegroundColor Yellow
Write-Host "   - Run: dotnet dev-certs https --clean" -ForegroundColor White
Write-Host "   - Run: dotnet dev-certs https --trust" -ForegroundColor White
Write-Host "   - Restart Visual Studio and browser" -ForegroundColor White
Write-Host ""

Write-Host "4. ?? Service Discovery Issues:" -ForegroundColor Yellow
Write-Host "   - Ensure appsettings.Development.json uses 'http://apiservice'" -ForegroundColor White
Write-Host "   - Verify Aspire AppHost is running and managing services" -ForegroundColor White
Write-Host "   - Check Aspire Dashboard for service status" -ForegroundColor White
Write-Host ""

Write-Host "5. ?? General Connection Issues:" -ForegroundColor Yellow
Write-Host "   - Restart the entire Aspire application" -ForegroundColor White
Write-Host "   - Clear browser cache and localStorage" -ForegroundColor White
Write-Host "   - Check Windows Firewall and antivirus software" -ForegroundColor White

Write-Host ""

# Step 9: Generate action plan
Write-Host "?? Recommended Action Plan:" -ForegroundColor Cyan

if ($hasErrors) {
    Write-Host "1. ? Fix critical issues identified above" -ForegroundColor Red
    Write-Host "2. ?? Restart the application completely:" -ForegroundColor Yellow
    Write-Host "   - Stop all processes (Ctrl+C in terminal)" -ForegroundColor White
    Write-Host "   - Run: dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor White
    Write-Host "3. ?? Test using the specific test script:" -ForegroundColor Yellow
    Write-Host "   .\scripts\test-dashboard-login.ps1" -ForegroundColor White
} else {
    Write-Host "1. ? No critical issues found!" -ForegroundColor Green
    Write-Host "2. ?? If still experiencing HttpRequestExceptions:" -ForegroundColor Yellow
    Write-Host "   - Check Aspire Dashboard: $aspireBase" -ForegroundColor White
    Write-Host "   - Review detailed logs in Visual Studio Output window" -ForegroundColor White
    Write-Host "   - Monitor network requests in browser Developer Tools" -ForegroundColor White
    Write-Host "3. ?? Test the full login flow:" -ForegroundColor Yellow
    Write-Host "   .\scripts\test-dashboard-login.ps1" -ForegroundColor White
}

Write-Host ""
Write-Host "?? Summary:" -ForegroundColor Cyan
Write-Host "- Aspire: $(if($aspireProcesses) { "? Running" } else { "? Not Running" })" -ForegroundColor White
Write-Host "- API Service: $(if($apiProcesses) { "? Running" } else { "? Not Running" })" -ForegroundColor White
Write-Host "- Web Service: $(if($webProcesses) { "? Running" } else { "? Not Running" })" -ForegroundColor White
Write-Host "- API HTTPS: $(if($apiHttpsWorking) { "? Working" } else { "? Not Working" })" -ForegroundColor White
Write-Host "- API HTTP: $(if($apiHttpWorking) { "? Working" } else { "? Not Working" })" -ForegroundColor White

Write-Host ""
Write-Host "? Connectivity diagnosis completed!" -ForegroundColor Green
Write-Host "?? Next: Fix any issues above, then test login at $webBase" -ForegroundColor Cyan