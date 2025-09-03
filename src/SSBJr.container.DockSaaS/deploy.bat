@echo off
REM DockSaaS Complete Build and Deploy Script for Windows
setlocal enabledelayedexpansion

set "RED=[91m"
set "GREEN=[92m"
set "YELLOW=[93m"
set "BLUE=[94m"
set "NC=[0m"

REM Function to print colored output
:print_status
echo %BLUE%[INFO]%NC% %~1
goto :eof

:print_success
echo %GREEN%[SUCCESS]%NC% %~1
goto :eof

:print_warning
echo %YELLOW%[WARNING]%NC% %~1
goto :eof

:print_error
echo %RED%[ERROR]%NC% %~1
goto :eof

REM Check dependencies
:check_dependencies
call :print_status "Checking dependencies..."

REM Check Docker
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    call :print_error "Docker is not installed or not in PATH"
    exit /b 1
)

REM Check Docker Compose
docker-compose --version >nul 2>&1
if %errorlevel% neq 0 (
    docker compose version >nul 2>&1
    if !errorlevel! neq 0 (
        call :print_error "Docker Compose is not installed"
        exit /b 1
    )
)

REM Check .NET SDK
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    call :print_error ".NET SDK is not installed"
    exit /b 1
)

REM Check .NET version
dotnet --version | findstr "8\." >nul
if %errorlevel% neq 0 (
    call :print_error ".NET 8 SDK is required"
    exit /b 1
)

call :print_success "All dependencies are available"
goto :eof

REM Clean build artifacts
:clean
call :print_status "Cleaning build artifacts..."

dotnet clean --configuration Release

REM Stop and remove Docker containers
for /f "tokens=*" %%i in ('docker ps -a --filter "name=docksaas" -q 2^>nul') do (
    docker stop %%i 2>nul
    docker rm %%i 2>nul
)

REM Remove Docker images
for /f "tokens=*" %%i in ('docker images --filter "reference=docksaas/*" -q 2^>nul') do (
    docker rmi %%i 2>nul
)

call :print_success "Clean completed"
goto :eof

REM Build .NET solution
:build_dotnet
call :print_status "Building .NET solution..."

dotnet restore
if %errorlevel% neq 0 (
    call :print_error "Package restore failed"
    exit /b 1
)

dotnet build --configuration Release --no-restore
if %errorlevel% neq 0 (
    call :print_error "Build failed"
    exit /b 1
)

dotnet test --configuration Release --no-build --verbosity minimal
if %errorlevel% neq 0 (
    call :print_error "Tests failed"
    exit /b 1
)

call :print_success ".NET build completed successfully"
goto :eof

REM Build Docker images
:build_docker
call :print_status "Building Docker images..."

call :print_status "Building API Service image..."
docker build -t docksaas/api:latest -f SSBJr.container.DockSaaS.ApiService/Dockerfile .
if %errorlevel% neq 0 (
    call :print_error "API Service build failed"
    exit /b 1
)

call :print_status "Building Web Service image..."
docker build -t docksaas/web:latest -f SSBJr.container.DockSaaS.Web/Dockerfile .
if %errorlevel% neq 0 (
    call :print_error "Web Service build failed"
    exit /b 1
)

REM Tag for different environments
docker tag docksaas/api:latest docksaas/api:dev
docker tag docksaas/api:latest docksaas/api:staging
docker tag docksaas/web:latest docksaas/web:dev
docker tag docksaas/web:latest docksaas/web:staging

call :print_success "Docker images built successfully"
goto :eof

REM Deploy development environment
:deploy_dev
call :print_status "Deploying development environment..."

docker-compose -f docker-compose.dev.yml up -d
if %errorlevel% neq 0 (
    call :print_error "Development deployment failed"
    exit /b 1
)

call :print_success "Development environment deployed"
call :print_status "Use 'dotnet run --project SSBJr.container.DockSaaS.AppHost' to start the application"
goto :eof

REM Deploy production environment
:deploy_prod
call :print_status "Deploying production environment..."

docker-compose up -d
if %errorlevel% neq 0 (
    call :print_error "Production deployment failed"
    exit /b 1
)

call :print_status "Waiting for services to be healthy..."
timeout /t 30 /nobreak >nul

call :print_success "Production environment deployed"
call :print_status "Access the application at:"
call :print_status "  Web Interface: http://localhost:3000"
call :print_status "  API Swagger:   http://localhost:5000/swagger"
call :print_status "  pgAdmin:       http://localhost:8080"
goto :eof

REM Deploy to Kubernetes
:deploy_k8s
call :print_status "Deploying to Kubernetes..."

kubectl cluster-info >nul 2>&1
if %errorlevel% neq 0 (
    call :print_error "Cannot connect to Kubernetes cluster"
    exit /b 1
)

call :print_status "Deploying infrastructure..."
kubectl apply -f k8s/infrastructure.yaml

call :print_status "Waiting for infrastructure..."
kubectl wait --for=condition=ready pod -l app=postgres -n docksaas --timeout=300s
kubectl wait --for=condition=ready pod -l app=redis -n docksaas --timeout=300s

call :print_status "Deploying applications..."
kubectl apply -f k8s/applications.yaml

call :print_status "Waiting for applications..."
kubectl wait --for=condition=ready pod -l app=docksaas-api -n docksaas --timeout=300s
kubectl wait --for=condition=ready pod -l app=docksaas-web -n docksaas --timeout=300s

call :print_success "Kubernetes deployment completed"
call :print_status "Access the application at: http://docksaas.local"
call :print_warning "Don't forget to add '127.0.0.1 docksaas.local' to your C:\Windows\System32\drivers\etc\hosts file"
goto :eof

REM Show status
:status
call :print_status "DockSaaS Status"
echo.

call :print_status "Docker Containers:"
docker ps --filter "name=docksaas" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo.

kubectl get pods -n docksaas 2>nul
if %errorlevel% equ 0 (
    call :print_status "Kubernetes Pods:"
    kubectl get pods -n docksaas
)
goto :eof

REM Stop services
:stop
call :print_status "Stopping DockSaaS services..."

if exist docker-compose.yml (
    docker-compose down
)

if exist docker-compose.dev.yml (
    docker-compose -f docker-compose.dev.yml down
)

kubectl delete namespace docksaas 2>nul

call :print_success "Services stopped"
goto :eof

REM Show logs
:logs
if "%~2"=="" (
    call :print_status "Showing logs for all services..."
    docker-compose logs -f
) else (
    call :print_status "Showing logs for %~2..."
    docker-compose logs -f %~2
)
goto :eof

REM Show help
:show_help
echo DockSaaS Build and Deploy Script
echo.
echo Usage: %~nx0 [COMMAND]
echo.
echo Commands:
echo   check       Check dependencies
echo   clean       Clean build artifacts and containers
echo   build       Build .NET solution and Docker images
echo   dev         Deploy development environment
echo   prod        Deploy production environment
echo   k8s         Deploy to Kubernetes
echo   status      Show status of services
echo   stop        Stop all services
echo   logs [svc]  Show logs (optionally for specific service)
echo   help        Show this help message
echo.
echo Examples:
echo   %~nx0 build        # Build everything
echo   %~nx0 dev          # Start development environment
echo   %~nx0 prod         # Start production environment
echo   %~nx0 k8s          # Deploy to Kubernetes
echo   %~nx0 logs api     # Show API service logs
goto :eof

REM Main script logic
:main
if "%~1"=="" goto show_help
if "%~1"=="check" goto check_dependencies
if "%~1"=="clean" goto clean
if "%~1"=="build" (
    call :check_dependencies
    if !errorlevel! neq 0 exit /b 1
    call :build_dotnet
    if !errorlevel! neq 0 exit /b 1
    call :build_docker
    if !errorlevel! neq 0 exit /b 1
    goto :eof
)
if "%~1"=="dev" (
    call :check_dependencies
    if !errorlevel! neq 0 exit /b 1
    call :build_docker
    if !errorlevel! neq 0 exit /b 1
    call :deploy_dev
    goto :eof
)
if "%~1"=="prod" (
    call :check_dependencies
    if !errorlevel! neq 0 exit /b 1
    call :build_docker
    if !errorlevel! neq 0 exit /b 1
    call :deploy_prod
    goto :eof
)
if "%~1"=="k8s" (
    call :check_dependencies
    if !errorlevel! neq 0 exit /b 1
    call :build_docker
    if !errorlevel! neq 0 exit /b 1
    call :deploy_k8s
    goto :eof
)
if "%~1"=="status" goto status
if "%~1"=="stop" goto stop
if "%~1"=="logs" goto logs
if "%~1"=="help" goto show_help
if "%~1"=="-h" goto show_help
if "%~1"=="--help" goto show_help

call :print_error "Unknown command: %~1"
goto show_help

REM Call main function
call :main %*