# Fix Dashboard JavaScript Dependencies
# This script helps diagnose and fix common JavaScript dependency issues in DockSaaS

Write-Host "?? DockSaaS Dashboard Diagnostics" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Function to check if file exists
function Test-FileExists {
    param($FilePath, $Description)
    if (Test-Path $FilePath) {
        Write-Host "? $Description exists: $FilePath" -ForegroundColor Green
        return $true
    } else {
        Write-Host "? $Description missing: $FilePath" -ForegroundColor Red
        return $false
    }
}

# Check project structure
Write-Host "`n?? Checking Project Structure..." -ForegroundColor Yellow

$webProject = "SSBJr.DockSaaS.Web"
$webRoot = "$webProject/wwwroot"
$jsFolder = "$webRoot/js"

Test-FileExists $webProject "Web Project"
Test-FileExists $webRoot "WWW Root"
Test-FileExists $jsFolder "JavaScript Folder"
Test-FileExists "$jsFolder/dashboard-charts.js" "Dashboard Charts Script"

# Check if Blazored.LocalStorage package is installed
Write-Host "`n?? Checking NuGet Packages..." -ForegroundColor Yellow

$csprojFile = "$webProject/SSBJr.DockSaaS.Web.csproj"
if (Test-Path $csprojFile) {
    $csprojContent = Get-Content $csprojFile -Raw
    if ($csprojContent -match "Blazored\.LocalStorage") {
        Write-Host "? Blazored.LocalStorage package is installed" -ForegroundColor Green
    } else {
        Write-Host "? Blazored.LocalStorage package not found" -ForegroundColor Red
        Write-Host "   Run: dotnet add package Blazored.LocalStorage" -ForegroundColor Yellow
    }
    
    if ($csprojContent -match "MudBlazor") {
        Write-Host "? MudBlazor package is installed" -ForegroundColor Green
    } else {
        Write-Host "? MudBlazor package not found" -ForegroundColor Red
        Write-Host "   Run: dotnet add package MudBlazor" -ForegroundColor Yellow
    }
} else {
    Write-Host "? Cannot find project file: $csprojFile" -ForegroundColor Red
}

# Check port configurations
Write-Host "`n?? Checking Port Configurations..." -ForegroundColor Yellow

$webLaunchSettings = "$webProject/Properties/launchSettings.json"
$apiLaunchSettings = "SSBJr.DockSaaS.ApiService/Properties/launchSettings.json"

if (Test-Path $webLaunchSettings) {
    $webSettings = Get-Content $webLaunchSettings -Raw | ConvertFrom-Json
    $httpsUrl = $webSettings.profiles.https.applicationUrl
    if ($httpsUrl -match "https://localhost:7001") {
        Write-Host "? Web application configured for port 7001" -ForegroundColor Green
    } else {
        Write-Host "??  Web application port mismatch. Expected: https://localhost:7001, Found: $httpsUrl" -ForegroundColor Yellow
    }
}

if (Test-Path $apiLaunchSettings) {
    $apiSettings = Get-Content $apiLaunchSettings -Raw | ConvertFrom-Json
    $httpsUrl = $apiSettings.profiles.https.applicationUrl
    if ($httpsUrl -match "https://localhost:7000") {
        Write-Host "? API service configured for port 7000" -ForegroundColor Green
    } else {
        Write-Host "??  API service port mismatch. Expected: https://localhost:7000, Found: $httpsUrl" -ForegroundColor Yellow
    }
}

# Check if services are running
Write-Host "`n?? Checking Running Services..." -ForegroundColor Yellow

try {
    $webResponse = Invoke-WebRequest -Uri "https://localhost:7001" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($webResponse.StatusCode -eq 200) {
        Write-Host "? Web application is running on https://localhost:7001" -ForegroundColor Green
    }
} catch {
    Write-Host "? Web application not responding on https://localhost:7001" -ForegroundColor Red
    Write-Host "   Try: dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor Yellow
}

try {
    $apiResponse = Invoke-WebRequest -Uri "https://localhost:7000/health" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($apiResponse.StatusCode -eq 200) {
        Write-Host "? API service is running on https://localhost:7000" -ForegroundColor Green
    }
} catch {
    Write-Host "? API service not responding on https://localhost:7000" -ForegroundColor Red
    Write-Host "   Try: dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor Yellow
}

# Provide solutions
Write-Host "`n?? Recommended Actions:" -ForegroundColor Yellow
Write-Host "======================" -ForegroundColor Yellow

Write-Host "1. Rebuild the solution:" -ForegroundColor White
Write-Host "   dotnet clean" -ForegroundColor Gray
Write-Host "   dotnet build" -ForegroundColor Gray

Write-Host "`n2. Clear browser cache and restart:" -ForegroundColor White
Write-Host "   - Press Ctrl+F5 in browser" -ForegroundColor Gray
Write-Host "   - Clear browser cache and cookies" -ForegroundColor Gray
Write-Host "   - Close all browser windows and restart" -ForegroundColor Gray

Write-Host "`n3. Restart application with Aspire:" -ForegroundColor White
Write-Host "   dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor Gray

Write-Host "`n4. Check Aspire Dashboard:" -ForegroundColor White
Write-Host "   https://localhost:17090" -ForegroundColor Gray

Write-Host "`n5. If errors persist, check browser console:" -ForegroundColor White
Write-Host "   - Open Developer Tools (F12)" -ForegroundColor Gray
Write-Host "   - Check Console tab for JavaScript errors" -ForegroundColor Gray
Write-Host "   - Check Network tab for failed requests" -ForegroundColor Gray

Write-Host "`n?? Common Issues and Solutions:" -ForegroundColor Yellow
Write-Host "==============================" -ForegroundColor Yellow

Write-Host "• blazored-localstorage.js 404 error:" -ForegroundColor White
Write-Host "  - Ensure Blazored.LocalStorage package is installed" -ForegroundColor Gray
Write-Host "  - Check if package reference exists in .csproj" -ForegroundColor Gray
Write-Host "  - Rebuild the project" -ForegroundColor Gray

Write-Host "`n• Message port closed error:" -ForegroundColor White
Write-Host "  - Browser extension interference" -ForegroundColor Gray
Write-Host "  - Try incognito/private browsing mode" -ForegroundColor Gray
Write-Host "  - Disable browser extensions temporarily" -ForegroundColor Gray

Write-Host "`n• Port mismatch errors:" -ForegroundColor White
Write-Host "  - Ensure applications run on correct ports" -ForegroundColor Gray
Write-Host "  - Web: https://localhost:7001" -ForegroundColor Gray
Write-Host "  - API: https://localhost:7000" -ForegroundColor Gray

Write-Host "`n? Dashboard should now work correctly!" -ForegroundColor Green
Write-Host "If issues persist, check the troubleshoot-login.ps1 script." -ForegroundColor Cyan

# End of script
Write-Host "`n?? Diagnostics completed!" -ForegroundColor Green