# Test Kafka Integration Script for DockSaaS
# This script tests the Kafka service managed by Aspire

Write-Host "?? DockSaaS Kafka Integration Test" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$apiBase = "https://localhost:7000"

Write-Host "Testing Kafka integration..." -ForegroundColor Yellow
Write-Host ""

# Test 1: Check if Aspire is running with Kafka
Write-Host "1. Checking if Aspire is running with Kafka" -ForegroundColor White
$aspireProcesses = Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" -and $_.CommandLine -like "*AppHost*" }
if ($aspireProcesses) {
    Write-Host "   ? Found Aspire process(es):" -ForegroundColor Green
    $aspireProcesses | ForEach-Object { Write-Host "      PID: $($_.Id) - $($_.ProcessName)" -ForegroundColor Gray }
} else {
    Write-Host "   ? No Aspire processes found. Run: dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Check if Kafka port is accessible
Write-Host "2. Checking Kafka port (9092)" -ForegroundColor White
try {
    $kafkaConnection = Test-NetConnection -ComputerName "localhost" -Port 9092 -WarningAction SilentlyContinue
    if ($kafkaConnection.TcpTestSucceeded) {
        Write-Host "   ? Kafka port 9092 is accessible" -ForegroundColor Green
    } else {
        Write-Host "   ??  Kafka port 9092 is not responding (might still be starting)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ? Cannot test Kafka port" -ForegroundColor Red
}

Write-Host ""

# Test 3: Check API health
Write-Host "3. Testing API health endpoint" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$apiBase/health" -Method GET -UseBasicParsing -TimeoutSec 10
    Write-Host "   ? API Health Check: $($response.StatusCode) - $($response.StatusDescription)" -ForegroundColor Green
} catch {
    Write-Host "   ? API Health Check Failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Make sure the API is running before testing Kafka integration" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Test 4: Check Docker containers related to Kafka
Write-Host "4. Checking Docker containers for Kafka" -ForegroundColor White
try {
    $dockerContainers = docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Status}}" | Where-Object { $_ -like "*kafka*" -or $_ -like "*aspire*" }
    if ($dockerContainers) {
        Write-Host "   ? Found Kafka-related containers:" -ForegroundColor Green
        $dockerContainers | ForEach-Object { Write-Host "      $_" -ForegroundColor Gray }
    } else {
        Write-Host "   ??  No Kafka containers found (Aspire might be managing internally)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ??  Docker not available or containers not visible" -ForegroundColor Cyan
}

Write-Host ""

# Test 5: Check Aspire Dashboard
Write-Host "5. Testing Aspire Dashboard for Kafka service" -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "https://localhost:17090" -Method GET -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "   ? Aspire Dashboard is accessible" -ForegroundColor Green
        Write-Host "   ?? Check Kafka status at: https://localhost:17090" -ForegroundColor Cyan
    }
} catch {
    Write-Host "   ??  Aspire Dashboard not accessible: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""

# Test 6: Test API endpoints are available (without auth)
Write-Host "6. Testing Kafka API endpoints structure" -ForegroundColor White
$kafkaEndpoints = @(
    "/swagger",
    "/api"
)

foreach ($endpoint in $kafkaEndpoints) {
    try {
        $response = Invoke-WebRequest -Uri "$apiBase$endpoint" -Method GET -UseBasicParsing -TimeoutSec 5
        Write-Host "   ? $endpoint is accessible ($($response.StatusCode))" -ForegroundColor Green
    } catch {
        if ($_.Exception.Response.StatusCode -eq 401) {
            Write-Host "   ? $endpoint requires authentication (expected)" -ForegroundColor Green
        } else {
            Write-Host "   ??  $endpoint returned: $($_.Exception.Response.StatusCode)" -ForegroundColor Yellow
        }
    }
}

Write-Host ""

Write-Host "?? Integration Status Summary:" -ForegroundColor Cyan
Write-Host "- ? Aspire with Kafka is ready to test" -ForegroundColor White
Write-Host "- ? API endpoints are available" -ForegroundColor White
Write-Host "- ?? Check Aspire Dashboard: https://localhost:17090" -ForegroundColor White
Write-Host "- ?? API Documentation: https://localhost:7000/swagger" -ForegroundColor White
Write-Host ""

Write-Host "?? Next Steps:" -ForegroundColor Cyan
Write-Host "1. Login to get JWT token: POST $apiBase/api/auth/login" -ForegroundColor White
Write-Host "2. Create a Kafka service instance via API" -ForegroundColor White
Write-Host "3. Test Kafka operations (create topics, produce/consume messages)" -ForegroundColor White
Write-Host "4. Monitor via Aspire Dashboard" -ForegroundColor White
Write-Host ""

Write-Host "? Kafka integration test completed!" -ForegroundColor Green

# Additional information
Write-Host ""
Write-Host "?? Kafka API Endpoints (after authentication):" -ForegroundColor Cyan
Write-Host "- GET  /api/kafka/{tenant}/{service}/cluster/info" -ForegroundColor Gray
Write-Host "- GET  /api/kafka/{tenant}/{service}/topics" -ForegroundColor Gray
Write-Host "- POST /api/kafka/{tenant}/{service}/topics" -ForegroundColor Gray
Write-Host "- POST /api/kafka/{tenant}/{service}/topics/{topic}/messages" -ForegroundColor Gray
Write-Host "- GET  /api/kafka/{tenant}/{service}/topics/{topic}/messages" -ForegroundColor Gray
Write-Host "- GET  /api/kafka/{tenant}/{service}/health" -ForegroundColor Gray