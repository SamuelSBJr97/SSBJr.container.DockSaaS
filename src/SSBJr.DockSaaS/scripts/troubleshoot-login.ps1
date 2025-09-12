# Script PowerShell para troubleshooting de problemas de login e dashboard no DockSaaS
# Execute este script se estiver enfrentando problemas de login ou dashboard

Write-Host "?? DockSaaS Login & Dashboard Troubleshooting" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se est� sendo executado a partir do diret�rio correto
$currentDir = Get-Location
Write-Host "?? Diret�rio atual: $currentDir" -ForegroundColor Yellow

# Fun��o para testar a acessibilidade de URLs
function Test-Url {
    param($Url, $Description)
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Host "? $Description acess�vel: $Url" -ForegroundColor Green
            return $true
        } else {
            Write-Host "??  $Description retornou status $($response.StatusCode): $Url" -ForegroundColor Yellow
            return $false
        }
    } catch {
        Write-Host "? $Description n�o acess�vel: $Url" -ForegroundColor Red
        Write-Host "   Erro: $($_.Exception.Message)" -ForegroundColor Gray
        return $false
    }
}

# Verificar servi�o da API
Write-Host "`n?? Verificando Servi�o da API..." -ForegroundColor Yellow
$apiRunning = Test-Url "https://localhost:7000/health" "Verifica��o de Sa�de da API"
if (-not $apiRunning) {
    Test-Url "http://localhost:5200/health" "Verifica��o de Sa�de da API (HTTP)"
}

# Verificar documenta��o Swagger
Write-Host "`n?? Verificando Documenta��o da API..." -ForegroundColor Yellow
Test-Url "https://localhost:7000/swagger" "Swagger UI"

# Verificar aplica��o Web
Write-Host "`n?? Verificando Aplica��o Web..." -ForegroundColor Yellow
$webRunning = Test-Url "https://localhost:7001" "Aplica��o Web"
if (-not $webRunning) {
    Test-Url "http://localhost:5201" "Aplica��o Web (HTTP)"
}

# Verificar Aspire Dashboard
Write-Host "`n??? Verificando Aspire Dashboard..." -ForegroundColor Yellow
Test-Url "https://localhost:17090" "Aspire Dashboard"

# Verificar processos em execu��o
Write-Host "`n?? Verificando Processos em Execu��o..." -ForegroundColor Yellow
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    Write-Host "? Encontrados $($dotnetProcesses.Count) processos .NET em execu��o" -ForegroundColor Green
    foreach ($process in $dotnetProcesses) {
        $commandLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($process.Id)" -ErrorAction SilentlyContinue).CommandLine
        if ($commandLine -like "*DockSaaS*") {
            Write-Host "   ?? Processo DockSaaS: PID $($process.Id)" -ForegroundColor Cyan
        }
    }
} else {
    Write-Host "? Nenhum processo .NET encontrado" -ForegroundColor Red
    Write-Host "   Voc� pode precisar iniciar a aplica��o" -ForegroundColor Yellow
}

# Verificar conflitos de porta comuns
Write-Host "`n?? Verificando Uso de Portas..." -ForegroundColor Yellow
$ports = @(7000, 7001, 5200, 5201, 17090)
foreach ($port in $ports) {
    try {
        $connection = Test-NetConnection -ComputerName "localhost" -Port $port -WarningAction SilentlyContinue -ErrorAction SilentlyContinue
        if ($connection.TcpTestSucceeded) {
            Write-Host "? Porta $port em uso (esperado)" -ForegroundColor Green
        } else {
            Write-Host "??  Porta $port n�o est� em uso" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "? N�o foi poss�vel verificar a porta $port" -ForegroundColor Gray
    }
}

# Verificar exist�ncia de arquivos
Write-Host "`n?? Verificando Arquivos do Projeto..." -ForegroundColor Yellow
$requiredFiles = @(
    "SSBJr.DockSaaS.AppHost/AppHost.cs",
    "SSBJr.DockSaaS.Web/Program.cs",
    "SSBJr.DockSaaS.ApiService/Program.cs",
    "SSBJr.DockSaaS.Web/wwwroot/js/dashboard-charts.js",
    "SSBJr.DockSaaS.Web/wwwroot/css/dashboard.css"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "? Encontrado: $file" -ForegroundColor Green
    } else {
        Write-Host "? Ausente: $file" -ForegroundColor Red
    }
}

# Verificar depend�ncias do JavaScript
Write-Host "`n?? Verificando Depend�ncias do JavaScript..." -ForegroundColor Yellow
$webCsproj = "SSBJr.DockSaaS.Web/SSBJr.DockSaaS.Web.csproj"
if (Test-Path $webCsproj) {
    $csprojContent = Get-Content $webCsproj -Raw
    if ($csprojContent -match "Blazored\.LocalStorage") {
        Write-Host "? Pacote Blazored.LocalStorage est� instalado" -ForegroundColor Green
    } else {
        Write-Host "? Pacote Blazored.LocalStorage n�o encontrado" -ForegroundColor Red
    }
    
    if ($csprojContent -match "MudBlazor") {
        Write-Host "? Pacote MudBlazor est� instalado" -ForegroundColor Green
    } else {
        Write-Host "? Pacote MudBlazor n�o encontrado" -ForegroundColor Red
    }
}

# Fornecer solu��es
Write-Host "`n?? Passos de Solu��o de Problemas:" -ForegroundColor Yellow
Write-Host "========================" -ForegroundColor Yellow

if (-not $apiRunning -and -not $webRunning) {
    Write-Host "?? Inicie a aplica��o:" -ForegroundColor White
    Write-Host "   dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "?? Solu��es Comuns:" -ForegroundColor White
Write-Host "1. Reinicie a aplica��o completamente" -ForegroundColor Gray
Write-Host "2. Limpe o cache e cookies do navegador" -ForegroundColor Gray
Write-Host "3. Tente o modo de navega��o an�nima/privada" -ForegroundColor Gray
Write-Host "4. Verifique as configura��es do Firewall do Windows" -ForegroundColor Gray
Write-Host "5. Verifique se o Docker Desktop est� em execu��o (para Aspire)" -ForegroundColor Gray

Write-Host "`n?? Informa��es de Login:" -ForegroundColor White
Write-Host "   URL: https://localhost:7001" -ForegroundColor Gray
Write-Host "   Email: admin@docksaas.com" -ForegroundColor Gray
Write-Host "   Senha: Admin123!" -ForegroundColor Gray
Write-Host "   Tenant: (deixe em branco para padr�o)" -ForegroundColor Gray

Write-Host "`n?? Problemas no Dashboard (erros blazored-localstorage.js):" -ForegroundColor White
Write-Host "   Esses erros geralmente s�o inofensivos e n�o afetam a funcionalidade:" -ForegroundColor Gray
Write-Host "   - 'blazored-localstorage.js 404' - Normal se o pacote carregar de forma diferente" -ForegroundColor Gray
Write-Host "   - 'message port closed' - Interfer�ncia de extens�o do navegador" -ForegroundColor Gray
Write-Host "   Solu��es:" -ForegroundColor Gray
Write-Host "   1. Desative as extens�es do navegador temporariamente" -ForegroundColor Gray
Write-Host "   2. Use o modo de navega��o an�nima/privada" -ForegroundColor Gray
Write-Host "   3. Limpe o cache do navegador (Ctrl+F5)" -ForegroundColor Gray
Write-Host "   4. Execute: .\scripts\fix-dashboard.ps1" -ForegroundColor Gray

Write-Host "`n?? Ajuda Adicional:" -ForegroundColor White
Write-Host "   - Verifique o Aspire Dashboard: https://localhost:17090" -ForegroundColor Gray
Write-Host "   - Veja a documenta��o da API: https://localhost:7000/swagger" -ForegroundColor Gray
Write-Host "   - Execute a corre��o do dashboard: .\scripts\fix-dashboard.ps1" -ForegroundColor Gray

Write-Host "`n? Se voc� pode ver a p�gina de login, a aplica��o est� funcionando corretamente!" -ForegroundColor Green
Write-Host "Erros de JavaScript no console geralmente s�o n�o cr�ticos." -ForegroundColor Cyan

# Fim do script
Write-Host "`n?? Troubleshooting conclu�do!" -ForegroundColor Green