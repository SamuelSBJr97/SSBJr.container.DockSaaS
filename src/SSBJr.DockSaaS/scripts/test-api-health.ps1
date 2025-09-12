# Test API Health Script for DockSaaS
# This script tests the API health endpoint and provides diagnostic information

Write-Host "?? DockSaaS API Health Test" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""

# Test API endpoints
$apiBase = "https://localhost:7000"
$apiHttp = "http://localhost:5200"

Write-Host "Testing API endpoints..." -ForegroundColor Yellow
Write-Host ""

# Test HTTPS endpoint
Write-Host "1. Testing HTTPS endpoint: $apiBase" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiBase/health" -Method GET -UseBasicParsing -TimeoutSec 10
    Write-Host "   ? HTTPS Health Check: $($response.StatusCode) - $($response.StatusDescription)" -ForegroundColor Green
    Write-Host "   ?? Response: $($response.Content.Substring(0, [Math]::Min(100, $response.Content.Length)))" -ForegroundColor Gray
} catch {
    Write-Host "   ? HTTPS Health Check Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test HTTP endpoint  
Write-Host "2. Testing HTTP endpoint: $apiHttp" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiHttp/health" -Method GET -UseBasicParsing -TimeoutSec 10
    Write-Host "   ? HTTP Health Check: $($response.StatusCode) - $($response.StatusDescription)" -ForegroundColor Green
    Write-Host "   ?? Response: $($response.Content.Substring(0, [Math]::Min(100, $response.Content.Length)))" -ForegroundColor Gray
} catch {
    Write-Host "   ? HTTP Health Check Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test Swagger endpoint
Write-Host "3. Testing Swagger endpoint" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiBase/swagger" -Method GET -UseBasicParsing -TimeoutSec 10
    Write-Host "   ? Swagger Available: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ? Swagger Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Check if Aspire is running
Write-Host "4. Checking if .NET Aspire is running" -ForegroundColor White
$aspireProcesses = Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" -and $_.CommandLine -like "*AppHost*" }
if ($aspireProcesses) {
    Write-Host "   ? Found Aspire process(es):" -ForegroundColor Green
    $aspireProcesses | ForEach-Object { Write-Host "      PID: $($_.Id) - $($_.ProcessName)" -ForegroundColor Gray }
} else {
    Write-Host "   ??  No Aspire processes found. Make sure to run:" -ForegroundColor Yellow
    Write-Host "      dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor Cyan
}

Write-Host ""

# Check Docker
Write-Host "5. Checking Docker status" -ForegroundColor White
try {
    $dockerInfo = docker info 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Docker is running" -ForegroundColor Green
    } else {
        Write-Host "   ??  Docker might not be running" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ??  Docker command not found or not running" -ForegroundColor Yellow
}

Write-Host ""

# Port check
Write-Host "6. Checking port availability" -ForegroundColor White
$ports = @(7000, 5200, 7001, 5201, 17090)
foreach ($port in $ports) {
    try {
        $connection = Test-NetConnection -ComputerName "localhost" -Port $port -WarningAction SilentlyContinue
        if ($connection.TcpTestSucceeded) {
            Write-Host "   ? Port $port is open" -ForegroundColor Green
        } else {
            Write-Host "   ? Port $port is not responding" -ForegroundColor Red
        }
    } catch {
        Write-Host "   ? Cannot test port $port" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "?? Recommendations:" -ForegroundColor Cyan
Write-Host "- If health checks fail, ensure Aspire is running" -ForegroundColor White
Write-Host "- If ports are closed, check firewall/antivirus" -ForegroundColor White
Write-Host "- Use CheckHealthAsync() method in Blazor (not GetAsync<object>)" -ForegroundColor White
Write-Host "- Check logs in Aspire Dashboard: https://localhost:17090" -ForegroundColor White
Write-Host ""
Write-Host "? Test completed!" -ForegroundColor Green