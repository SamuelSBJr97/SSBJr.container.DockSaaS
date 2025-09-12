# Script PowerShell para aplicar migrations no PostgreSQL containerizado
# Execute este script após subir o container do PostgreSQL

Write-Host "?? Iniciando aplicação de migrations no PostgreSQL containerizado..." -ForegroundColor Green

# Verificar se o Docker está rodando
try {
    docker info | Out-Null
} catch {
    Write-Host "? Docker não está rodando. Inicie o Docker primeiro." -ForegroundColor Red
    exit 1
}

# Subir apenas o PostgreSQL
Write-Host "?? Subindo container PostgreSQL..." -ForegroundColor Yellow
docker-compose up -d postgres

# Aguardar PostgreSQL estar pronto
Write-Host "? Aguardando PostgreSQL ficar pronto..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Verificar se PostgreSQL está rodando
$postgresStatus = docker-compose ps postgres
if (-not ($postgresStatus -match "Up")) {
    Write-Host "? PostgreSQL não está rodando. Verifique os logs com: docker-compose logs postgres" -ForegroundColor Red
    exit 1
}

Write-Host "? PostgreSQL está rodando!" -ForegroundColor Green

# Aplicar migrations
Write-Host "?? Aplicando migrations..." -ForegroundColor Yellow
Set-Location "src/SSBJr.container.DockSaaS"

# Executar migrations
$migrationResult = dotnet ef database update --project SSBJr.container.DockSaaS.ApiService/SSBJr.container.DockSaaS.ApiService.csproj

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Migrations aplicadas com sucesso!" -ForegroundColor Green
    Write-Host "?? Banco de dados está pronto para uso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Para acessar o pgAdmin:" -ForegroundColor Cyan
    Write-Host "   URL: http://localhost:8080" -ForegroundColor White
    Write-Host "   Email: admin@docksaas.com" -ForegroundColor White
    Write-Host "   Senha: admin" -ForegroundColor White
    Write-Host ""
    Write-Host "?? Para parar os containers:" -ForegroundColor Cyan
    Write-Host "   docker-compose down" -ForegroundColor White
} else {
    Write-Host "? Erro ao aplicar migrations. Verifique os logs acima." -ForegroundColor Red
    exit 1
}