# Script PowerShell para troubleshooting de problemas de login e dashboard no DockSaaS
# Execute este script se estiver enfrentando problemas de login ou dashboard

Write-Host "?? DockSaaS Login & Dashboard Troubleshooting" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se está sendo executado a partir do diretório correto
$currentDir = Get-Location
Write-Host "?? Diretório atual: $currentDir" -ForegroundColor Yellow

# Função para testar a acessibilidade de URLs
function Test-Url {
    param($Url, $Description)
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Host "? $Description acessível: $Url" -ForegroundColor Green
            return $true
        } else {
            Write-Host "??  $Description retornou status $($response.StatusCode): $Url" -ForegroundColor Yellow
            return $false
        }
    } catch {
        Write-Host "? $Description não acessível: $Url" -ForegroundColor Red
        Write-Host "   Erro: $($_.Exception.Message)" -ForegroundColor Gray
        return $false
    }
}

# Verificar serviço da API
Write-Host "`n?? Verificando Serviço da API..." -ForegroundColor Yellow
$apiRunning = Test-Url "https://localhost:7000/health" "Verificação de Saúde da API"
if (-not $apiRunning) {
    Test-Url "http://localhost:5200/health" "Verificação de Saúde da API (HTTP)"
}

# Verificar documentação Swagger
Write-Host "`n?? Verificando Documentação da API..." -ForegroundColor Yellow
Test-Url "https://localhost:7000/swagger" "Swagger UI"

# Verificar aplicação Web
Write-Host "`n?? Verificando Aplicação Web..." -ForegroundColor Yellow
$webRunning = Test-Url "https://localhost:7001" "Aplicação Web"
if (-not $webRunning) {
    Test-Url "http://localhost:5201" "Aplicação Web (HTTP)"
}

# Verificar Aspire Dashboard
Write-Host "`n??? Verificando Aspire Dashboard..." -ForegroundColor Yellow
Test-Url "https://localhost:17090" "Aspire Dashboard"

# Verificar processos em execução
Write-Host "`n?? Verificando Processos em Execução..." -ForegroundColor Yellow
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    Write-Host "? Encontrados $($dotnetProcesses.Count) processos .NET em execução" -ForegroundColor Green
    foreach ($process in $dotnetProcesses) {
        $commandLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($process.Id)" -ErrorAction SilentlyContinue).CommandLine
        if ($commandLine -like "*DockSaaS*") {
            Write-Host "   ?? Processo DockSaaS: PID $($process.Id)" -ForegroundColor Cyan
        }
    }
} else {
    Write-Host "? Nenhum processo .NET encontrado" -ForegroundColor Red
    Write-Host "   Você pode precisar iniciar a aplicação" -ForegroundColor Yellow
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
            Write-Host "??  Porta $port não está em uso" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "? Não foi possível verificar a porta $port" -ForegroundColor Gray
    }
}

# Verificar existência de arquivos
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

# Verificar dependências do JavaScript
Write-Host "`n?? Verificando Dependências do JavaScript..." -ForegroundColor Yellow
$webCsproj = "SSBJr.DockSaaS.Web/SSBJr.DockSaaS.Web.csproj"
if (Test-Path $webCsproj) {
    $csprojContent = Get-Content $webCsproj -Raw
    if ($csprojContent -match "Blazored\.LocalStorage") {
        Write-Host "? Pacote Blazored.LocalStorage está instalado" -ForegroundColor Green
    } else {
        Write-Host "? Pacote Blazored.LocalStorage não encontrado" -ForegroundColor Red
    }
    
    if ($csprojContent -match "MudBlazor") {
        Write-Host "? Pacote MudBlazor está instalado" -ForegroundColor Green
    } else {
        Write-Host "? Pacote MudBlazor não encontrado" -ForegroundColor Red
    }
}

# Fornecer soluções
Write-Host "`n?? Passos de Solução de Problemas:" -ForegroundColor Yellow
Write-Host "========================" -ForegroundColor Yellow

if (-not $apiRunning -and -not $webRunning) {
    Write-Host "?? Inicie a aplicação:" -ForegroundColor White
    Write-Host "   dotnet run --project SSBJr.DockSaaS.AppHost" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "?? Soluções Comuns:" -ForegroundColor White
Write-Host "1. Reinicie a aplicação completamente" -ForegroundColor Gray
Write-Host "2. Limpe o cache e cookies do navegador" -ForegroundColor Gray
Write-Host "3. Tente o modo de navegação anônima/privada" -ForegroundColor Gray
Write-Host "4. Verifique as configurações do Firewall do Windows" -ForegroundColor Gray
Write-Host "5. Verifique se o Docker Desktop está em execução (para Aspire)" -ForegroundColor Gray

Write-Host "`n?? Informações de Login:" -ForegroundColor White
Write-Host "   URL: https://localhost:7001" -ForegroundColor Gray
Write-Host "   Email: admin@docksaas.com" -ForegroundColor Gray
Write-Host "   Senha: Admin123!" -ForegroundColor Gray
Write-Host "   Tenant: (deixe em branco para padrão)" -ForegroundColor Gray

Write-Host "`n?? Problemas no Dashboard (erros blazored-localstorage.js):" -ForegroundColor White
Write-Host "   Esses erros geralmente são inofensivos e não afetam a funcionalidade:" -ForegroundColor Gray
Write-Host "   - 'blazored-localstorage.js 404' - Normal se o pacote carregar de forma diferente" -ForegroundColor Gray
Write-Host "   - 'message port closed' - Interferência de extensão do navegador" -ForegroundColor Gray
Write-Host "   Soluções:" -ForegroundColor Gray
Write-Host "   1. Desative as extensões do navegador temporariamente" -ForegroundColor Gray
Write-Host "   2. Use o modo de navegação anônima/privada" -ForegroundColor Gray
Write-Host "   3. Limpe o cache do navegador (Ctrl+F5)" -ForegroundColor Gray
Write-Host "   4. Execute: .\scripts\fix-dashboard.ps1" -ForegroundColor Gray

Write-Host "`n?? Ajuda Adicional:" -ForegroundColor White
Write-Host "   - Verifique o Aspire Dashboard: https://localhost:17090" -ForegroundColor Gray
Write-Host "   - Veja a documentação da API: https://localhost:7000/swagger" -ForegroundColor Gray
Write-Host "   - Execute a correção do dashboard: .\scripts\fix-dashboard.ps1" -ForegroundColor Gray

Write-Host "`n? Se você pode ver a página de login, a aplicação está funcionando corretamente!" -ForegroundColor Green
Write-Host "Erros de JavaScript no console geralmente são não críticos." -ForegroundColor Cyan

# Fim do script
Write-Host "`n?? Troubleshooting concluído!" -ForegroundColor Green