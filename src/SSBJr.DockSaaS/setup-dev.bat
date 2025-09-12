@echo off
REM Development setup script for DockSaaS (Windows)

echo ?? Setting up DockSaaS development environment...

REM Check if .NET 8 is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ? .NET 8 SDK is required. Please install it from https://dotnet.microsoft.com/download/dotnet/8.0
    exit /b 1
)

REM Check if Docker is installed
docker --version >nul 2>&1
if errorlevel 1 (
    echo ? Docker is required. Please install Docker Desktop from https://www.docker.com/products/docker-desktop
    exit /b 1
)

echo ? Prerequisites check passed

REM Restore NuGet packages
echo ?? Restoring NuGet packages...
dotnet restore

REM Start PostgreSQL and Redis with Docker
echo ?? Starting PostgreSQL and Redis containers...
docker run -d --name docksaas-postgres -p 5432:5432 -e POSTGRES_DB=docksaas -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres postgres:15
docker run -d --name docksaas-redis -p 6379:6379 redis:7-alpine

REM Wait for containers to be ready
echo ? Waiting for containers to be ready...
timeout /t 10 /nobreak >nul

REM Build the solution
echo ?? Building the solution...
dotnet build

REM Run database migrations
echo ??? Setting up database...
cd SSBJr.DockSaaS.ApiService
dotnet ef database update
cd ..

echo ? Development environment setup complete!
echo.
echo ?? To start the application:
echo    dotnet run --project SSBJr.DockSaaS.AppHost
echo.
echo ?? Access points:
echo    � Aspire Dashboard: https://localhost:15888
echo    � Web Interface: https://localhost:7001
echo    � API Service: https://localhost:7000
echo    � API Documentation: https://localhost:7000/swagger
echo.
echo ?? To clean up containers:
echo    docker stop docksaas-postgres docksaas-redis
echo    docker rm docksaas-postgres docksaas-redis

pause