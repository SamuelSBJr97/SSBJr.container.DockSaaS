#!/bin/bash

# Script para aplicar migrations no PostgreSQL containerizado
# Execute este script ap�s subir o container do PostgreSQL

echo "?? Iniciando aplica��o de migrations no PostgreSQL containerizado..."

# Verificar se o Docker est� rodando
if ! docker info > /dev/null 2>&1; then
    echo "? Docker n�o est� rodando. Inicie o Docker primeiro."
    exit 1
fi

# Subir apenas o PostgreSQL
echo "?? Subindo container PostgreSQL..."
docker-compose up -d postgres

# Aguardar PostgreSQL estar pronto
echo "? Aguardando PostgreSQL ficar pronto..."
sleep 10

# Verificar se PostgreSQL est� rodando
if ! docker-compose ps postgres | grep -q "Up"; then
    echo "? PostgreSQL n�o est� rodando. Verifique os logs com: docker-compose logs postgres"
    exit 1
fi

echo "? PostgreSQL est� rodando!"

# Aplicar migrations
echo "?? Aplicando migrations..."
cd src/SSBJr.DockSaaS

# Executar migrations
dotnet ef database update --project SSBJr.DockSaaS.ApiService/SSBJr.DockSaaS.ApiService.csproj

if [ $? -eq 0 ]; then
    echo "? Migrations aplicadas com sucesso!"
    echo "?? Banco de dados est� pronto para uso!"
    echo ""
    echo "?? Para acessar o pgAdmin:"
    echo "   URL: http://localhost:8080"
    echo "   Email: admin@docksaas.com"
    echo "   Senha: admin"
    echo ""
    echo "?? Para parar os containers:"
    echo "   docker-compose down"
else
    echo "? Erro ao aplicar migrations. Verifique os logs acima."
    exit 1
fi