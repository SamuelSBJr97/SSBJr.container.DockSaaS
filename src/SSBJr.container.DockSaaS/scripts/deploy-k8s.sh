#!/bin/bash

# Deploy DockSaaS to Kubernetes

set -e

echo "?? Deploying DockSaaS to Kubernetes..."

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "? kubectl is not installed or not in PATH"
    exit 1
fi

# Check if cluster is accessible
if ! kubectl cluster-info &> /dev/null; then
    echo "? Cannot connect to Kubernetes cluster"
    exit 1
fi

echo "? Kubernetes cluster is accessible"

# Apply infrastructure
echo "?? Deploying infrastructure..."
kubectl apply -f k8s/infrastructure.yaml

# Wait for infrastructure to be ready
echo "? Waiting for infrastructure to be ready..."
kubectl wait --for=condition=ready pod -l app=postgres -n docksaas --timeout=300s
kubectl wait --for=condition=ready pod -l app=redis -n docksaas --timeout=300s

# Apply applications
echo "?? Deploying applications..."
kubectl apply -f k8s/applications.yaml

# Wait for applications to be ready
echo "? Waiting for applications to be ready..."
kubectl wait --for=condition=ready pod -l app=docksaas-api -n docksaas --timeout=300s
kubectl wait --for=condition=ready pod -l app=docksaas-web -n docksaas --timeout=300s

echo "? Deployment completed successfully!"

# Show deployment status
echo ""
echo "?? Deployment Status:"
kubectl get pods -n docksaas
echo ""
kubectl get services -n docksaas
echo ""
kubectl get ingress -n docksaas

echo ""
echo "?? Application URLs:"
echo "  Web Interface: http://docksaas.local"
echo "  API Swagger:   http://docksaas.local/api/swagger"
echo ""
echo "?? Add '127.0.0.1 docksaas.local' to your /etc/hosts file to access the application"