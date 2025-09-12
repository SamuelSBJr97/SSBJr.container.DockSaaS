@echo off
REM Build and deploy DockSaaS to Docker

echo ?? Building DockSaaS Docker Images...

REM Build API Service
echo ?? Building API Service...
docker build -t docksaas/api:latest -f SSBJr.DockSaaS.ApiService/Dockerfile .
if %ERRORLEVEL% neq 0 (
    echo ? Failed to build API Service
    exit /b 1
)

REM Build Web Service
echo ?? Building Web Service...
docker build -t docksaas/web:latest -f SSBJr.DockSaaS.Web/Dockerfile .
if %ERRORLEVEL% neq 0 (
    echo ? Failed to build Web Service
    exit /b 1
)

echo ? Docker images built successfully!

REM Tag images for different environments
echo ??? Tagging images for different environments...
docker tag docksaas/api:latest docksaas/api:dev
docker tag docksaas/api:latest docksaas/api:staging
docker tag docksaas/web:latest docksaas/web:dev
docker tag docksaas/web:latest docksaas/web:staging

echo ?? Available images:
docker images | findstr docksaas

echo.
echo ?? To run the application:
echo   Development: docker-compose -f docker-compose.dev.yml up -d
echo   Production:  docker-compose up -d
echo.
echo ?? To deploy to Kubernetes:
echo   kubectl apply -f k8s/infrastructure.yaml
echo   kubectl apply -f k8s/applications.yaml

pause