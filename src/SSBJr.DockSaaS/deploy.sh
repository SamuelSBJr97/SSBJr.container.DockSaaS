#!/bin/bash

# DockSaaS Complete Build and Deploy Script
# This script handles all deployment scenarios

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check dependencies
check_dependencies() {
    print_status "Checking dependencies..."
    
    # Check Docker
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed or not in PATH"
        exit 1
    fi
    
    # Check Docker Compose
    if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
        print_error "Docker Compose is not installed"
        exit 1
    fi
    
    # Check .NET SDK
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed"
        exit 1
    fi
    
    # Check .NET version
    if ! dotnet --version | grep -q "8\."; then
        print_error ".NET 8 SDK is required"
        exit 1
    fi
    
    print_success "All dependencies are available"
}

# Clean build artifacts
clean() {
    print_status "Cleaning build artifacts..."
    
    # Clean .NET artifacts
    dotnet clean --configuration Release
    
    # Remove Docker containers and images
    if docker ps -a --filter "name=docksaas" -q | wc -l > 0; then
        docker stop $(docker ps -a --filter "name=docksaas" -q) 2>/dev/null || true
        docker rm $(docker ps -a --filter "name=docksaas" -q) 2>/dev/null || true
    fi
    
    # Remove Docker images
    if docker images --filter "reference=docksaas/*" -q | wc -l > 0; then
        docker rmi $(docker images --filter "reference=docksaas/*" -q) 2>/dev/null || true
    fi
    
    print_success "Clean completed"
}

# Build .NET solution
build_dotnet() {
    print_status "Building .NET solution..."
    
    # Restore packages
    dotnet restore
    
    # Build solution
    dotnet build --configuration Release --no-restore
    
    # Run tests
    dotnet test --configuration Release --no-build --verbosity minimal
    
    print_success ".NET build completed successfully"
}

# Build Docker images
build_docker() {
    print_status "Building Docker images..."
    
    # Build API Service
    print_status "Building API Service image..."
    docker build -t docksaas/api:latest -f SSBJr.DockSaaS.ApiService/Dockerfile .
    
    # Build Web Service
    print_status "Building Web Service image..."
    docker build -t docksaas/web:latest -f SSBJr.DockSaaS.Web/Dockerfile .
    
    # Tag for different environments
    docker tag docksaas/api:latest docksaas/api:dev
    docker tag docksaas/api:latest docksaas/api:staging
    docker tag docksaas/web:latest docksaas/web:dev
    docker tag docksaas/web:latest docksaas/web:staging
    
    print_success "Docker images built successfully"
}

# Deploy development environment
deploy_dev() {
    print_status "Deploying development environment..."
    
    # Start infrastructure only
    docker-compose -f docker-compose.dev.yml up -d
    
    print_success "Development environment deployed"
    print_status "Use 'dotnet run --project SSBJr.DockSaaS.AppHost' to start the application"
}

# Deploy production environment
deploy_prod() {
    print_status "Deploying production environment..."
    
    # Start all services
    docker-compose up -d
    
    # Wait for services to be healthy
    print_status "Waiting for services to be healthy..."
    sleep 30
    
    # Check service health
    for service in docksaas-postgres docksaas-redis docksaas-api docksaas-web; do
        if docker ps --filter "name=$service" --filter "status=running" | grep -q $service; then
            print_success "$service is running"
        else
            print_error "$service failed to start"
            docker logs $service --tail 50
        fi
    done
    
    print_success "Production environment deployed"
    print_status "Access the application at:"
    print_status "  Web Interface: http://localhost:3000"
    print_status "  API Swagger:   http://localhost:5000/swagger"
    print_status "  pgAdmin:       http://localhost:8080"
}

# Deploy to Kubernetes
deploy_k8s() {
    print_status "Deploying to Kubernetes..."
    
    # Check kubectl
    if ! command -v kubectl &> /dev/null; then
        print_error "kubectl is not installed"
        exit 1
    fi
    
    # Check cluster connectivity
    if ! kubectl cluster-info &> /dev/null; then
        print_error "Cannot connect to Kubernetes cluster"
        exit 1
    fi
    
    # Apply infrastructure
    print_status "Deploying infrastructure..."
    kubectl apply -f k8s/infrastructure.yaml
    
    # Wait for infrastructure
    print_status "Waiting for infrastructure..."
    kubectl wait --for=condition=ready pod -l app=postgres -n docksaas --timeout=300s
    kubectl wait --for=condition=ready pod -l app=redis -n docksaas --timeout=300s
    
    # Apply applications
    print_status "Deploying applications..."
    kubectl apply -f k8s/applications.yaml
    
    # Wait for applications
    print_status "Waiting for applications..."
    kubectl wait --for=condition=ready pod -l app=docksaas-api -n docksaas --timeout=300s
    kubectl wait --for=condition=ready pod -l app=docksaas-web -n docksaas --timeout=300s
    
    print_success "Kubernetes deployment completed"
    print_status "Access the application at: http://docksaas.local"
    print_warning "Don't forget to add '127.0.0.1 docksaas.local' to your /etc/hosts file"
}

# Show status
status() {
    print_status "DockSaaS Status"
    echo
    
    # Docker status
    print_status "Docker Containers:"
    if docker ps --filter "name=docksaas" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | tail -n +2 | wc -l > 0; then
        docker ps --filter "name=docksaas" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    else
        print_warning "No Docker containers running"
    fi
    
    echo
    
    # Kubernetes status
    if command -v kubectl &> /dev/null && kubectl cluster-info &> /dev/null; then
        print_status "Kubernetes Pods:"
        if kubectl get pods -n docksaas 2>/dev/null | tail -n +2 | wc -l > 0; then
            kubectl get pods -n docksaas
        else
            print_warning "No Kubernetes pods found"
        fi
    fi
}

# Stop services
stop() {
    print_status "Stopping DockSaaS services..."
    
    # Stop Docker Compose
    if [ -f "docker-compose.yml" ]; then
        docker-compose down
    fi
    
    if [ -f "docker-compose.dev.yml" ]; then
        docker-compose -f docker-compose.dev.yml down
    fi
    
    # Stop Kubernetes
    if command -v kubectl &> /dev/null && kubectl cluster-info &> /dev/null; then
        kubectl delete namespace docksaas 2>/dev/null || true
    fi
    
    print_success "Services stopped"
}

# Show logs
logs() {
    service=${1:-"all"}
    
    if [ "$service" = "all" ]; then
        print_status "Showing logs for all services..."
        docker-compose logs -f
    else
        print_status "Showing logs for $service..."
        docker-compose logs -f $service
    fi
}

# Show help
show_help() {
    echo "DockSaaS Build and Deploy Script"
    echo
    echo "Usage: $0 [COMMAND]"
    echo
    echo "Commands:"
    echo "  check       Check dependencies"
    echo "  clean       Clean build artifacts and containers"
    echo "  build       Build .NET solution and Docker images"
    echo "  dev         Deploy development environment"
    echo "  prod        Deploy production environment"
    echo "  k8s         Deploy to Kubernetes"
    echo "  status      Show status of services"
    echo "  stop        Stop all services"
    echo "  logs [svc]  Show logs (optionally for specific service)"
    echo "  help        Show this help message"
    echo
    echo "Examples:"
    echo "  $0 build        # Build everything"
    echo "  $0 dev          # Start development environment"
    echo "  $0 prod         # Start production environment"
    echo "  $0 k8s          # Deploy to Kubernetes"
    echo "  $0 logs api     # Show API service logs"
}

# Main script logic
main() {
    case "${1:-help}" in
        check)
            check_dependencies
            ;;
        clean)
            clean
            ;;
        build)
            check_dependencies
            build_dotnet
            build_docker
            ;;
        dev)
            check_dependencies
            build_docker
            deploy_dev
            ;;
        prod)
            check_dependencies
            build_docker
            deploy_prod
            ;;
        k8s)
            check_dependencies
            build_docker
            deploy_k8s
            ;;
        status)
            status
            ;;
        stop)
            stop
            ;;
        logs)
            logs "$2"
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            print_error "Unknown command: $1"
            show_help
            exit 1
            ;;
    esac
}

# Run main function
main "$@"