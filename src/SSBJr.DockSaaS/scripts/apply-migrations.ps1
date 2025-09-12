# Script PowerShell para aplicar migrations no PostgreSQL containerizado
# Execute este script ap�s subir o container do PostgreSQL

Write-Host "?? Iniciando aplica��o de migrations no PostgreSQL containerizado..." -ForegroundColor Green

# Verificar se o Docker est� rodando
try {
    docker info | Out-Null
} catch {
    Write-Host "? Docker n�o est� rodando. Inicie o Docker primeiro." -ForegroundColor Red
    exit 1
}

# Subir apenas o PostgreSQL
Write-Host "?? Subindo container PostgreSQL..." -ForegroundColor Yellow
docker-compose up -d postgres

# Aguardar PostgreSQL estar pronto
Write-Host "? Aguardando PostgreSQL ficar pronto..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Verificar se PostgreSQL est� rodando
$postgresStatus = docker-compose ps postgres
if (-not ($postgresStatus -match "Up")) {
    Write-Host "? PostgreSQL n�o est� rodando. Verifique os logs com: docker-compose logs postgres" -ForegroundColor Red
    exit 1
}

Write-Host "? PostgreSQL est� rodando!" -ForegroundColor Green

# Aplicar migrations
Write-Host "?? Aplicando migrations..." -ForegroundColor Yellow
Set-Location "src/SSBJr.DockSaaS"

# Executar migrations
$migrationResult = dotnet ef database update --project SSBJr.DockSaaS.ApiService/SSBJr.DockSaaS.ApiService.csproj

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Migrations aplicadas com sucesso!" -ForegroundColor Green
    Write-Host "?? Banco de dados est� pronto para uso!" -ForegroundColor Green
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