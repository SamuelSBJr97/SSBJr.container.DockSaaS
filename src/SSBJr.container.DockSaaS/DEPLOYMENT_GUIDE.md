# DockSaaS Deployment Guide

Este guia fornece instruções detalhadas para deploy da plataforma DockSaaS usando Docker e Kubernetes.

## ?? Pré-requisitos

### Para Docker
- Docker Desktop 4.0+
- Docker Compose 2.0+
- 4GB RAM disponível
- 10GB espaço em disco

### Para Kubernetes
- Kubernetes cluster (minikube, Docker Desktop, ou cloud provider)
- kubectl configurado
- NGINX Ingress Controller instalado
- 8GB RAM disponível no cluster
- 20GB espaço em disco

## ?? Deploy com Docker Compose

### 1. Desenvolvimento Local

```bash
# Clone o repositório
git clone https://github.com/SamuelSBJr97/SSBJr.container.DockSaaS.git
cd SSBJr.container.DockSaaS

# Inicie apenas a infraestrutura para desenvolvimento
docker-compose -f docker-compose.dev.yml up -d

# Execute a aplicação via .NET Aspire
dotnet run --project SSBJr.container.DockSaaS.AppHost
```

**URLs de Desenvolvimento:**
- Aspire Dashboard: https://localhost:15888
- Web Interface: https://localhost:7001
- API Swagger: https://localhost:7000/swagger
- pgAdmin: http://localhost:8080

### 2. Produção Docker

```bash
# Build das imagens
chmod +x scripts/build-docker.sh
./scripts/build-docker.sh

# Deploy completo
docker-compose up -d

# Verificar status
docker-compose ps
docker-compose logs -f
```

**URLs de Produção:**
- Web Interface: http://localhost:3000
- API Swagger: http://localhost:5000/swagger
- pgAdmin: http://localhost:8080
- Redis Commander: http://localhost:8081

## ?? Deploy com Kubernetes

### 1. Preparar Cluster

```bash
# Para minikube
minikube start --memory=8192 --cpus=4
minikube addons enable ingress

# Para Docker Desktop
# Habilitar Kubernetes nas configurações

# Instalar NGINX Ingress (se necessário)
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/cloud/deploy.yaml
```

### 2. Build e Push das Imagens

```bash
# Build local
./scripts/build-docker.sh

# Para registry remoto (opcional)
docker tag docksaas/api:latest your-registry/docksaas/api:latest
docker tag docksaas/web:latest your-registry/docksaas/web:latest
docker push your-registry/docksaas/api:latest
docker push your-registry/docksaas/web:latest
```

### 3. Deploy no Kubernetes

```bash
# Deploy automático
chmod +x scripts/deploy-k8s.sh
./scripts/deploy-k8s.sh

# OU deploy manual
kubectl apply -f k8s/infrastructure.yaml
kubectl wait --for=condition=ready pod -l app=postgres -n docksaas --timeout=300s
kubectl apply -f k8s/applications.yaml
```

### 4. Configurar Acesso

```bash
# Adicionar ao /etc/hosts (Linux/Mac)
echo "127.0.0.1 docksaas.local" | sudo tee -a /etc/hosts

# Windows (executar como Administrador)
echo 127.0.0.1 docksaas.local >> C:\Windows\System32\drivers\etc\hosts
```

**URLs Kubernetes:**
- Web Interface: http://docksaas.local
- API Swagger: http://docksaas.local/api/swagger

## ?? Configuração

### Variáveis de Ambiente

#### API Service
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=postgres;Database=docksaasdb;Username=postgres;Password=postgres;
ConnectionStrings__Redis=redis:6379
JwtSettings__Secret=your-secret-key
JwtSettings__Issuer=DockSaaS
JwtSettings__Audience=DockSaaSUsers
```

#### Web Service
```env
ASPNETCORE_ENVIRONMENT=Production
ApiBaseUrl=http://docksaas-api:8080
ConnectionStrings__Redis=redis:6379
```

### Secrets

Para produção, use secrets gerenciados:

```bash
# Kubernetes
kubectl create secret generic docksaas-secrets \
  --from-literal=db-password=your-secure-password \
  --from-literal=jwt-secret=your-jwt-secret \
  -n docksaas

# Docker Compose
echo "your-secure-password" | docker secret create db_password -
```

## ?? Monitoramento

### Health Checks

```bash
# Docker
curl http://localhost:5000/health
curl http://localhost:3000/health

# Kubernetes
kubectl get pods -n docksaas
kubectl describe pod <pod-name> -n docksaas
```

### Logs

```bash
# Docker
docker-compose logs -f docksaas-api
docker-compose logs -f docksaas-web

# Kubernetes
kubectl logs -f deployment/docksaas-api -n docksaas
kubectl logs -f deployment/docksaas-web -n docksaas
```

### Métricas

```bash
# Status dos serviços
kubectl get all -n docksaas

# Uso de recursos
kubectl top pods -n docksaas
kubectl top nodes
```

## ?? Segurança

### 1. Configuração TLS

```yaml
# Para Kubernetes com cert-manager
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: your-email@domain.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
```

### 2. Network Policies

```yaml
# Exemplo de network policy
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: docksaas-network-policy
  namespace: docksaas
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: docksaas
```

## ?? Escalabilidade

### Horizontal Pod Autoscaler

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: docksaas-api-hpa
  namespace: docksaas
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: docksaas-api
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

### Database Scaling

```bash
# PostgreSQL com réplicas de leitura
kubectl apply -f k8s/postgres-replica.yaml
```

## ?? Troubleshooting

### Problemas Comuns

1. **Pod não inicia**
```bash
kubectl describe pod <pod-name> -n docksaas
kubectl logs <pod-name> -n docksaas
```

2. **Conexão com banco**
```bash
kubectl exec -it <postgres-pod> -n docksaas -- psql -U postgres -d docksaasdb
```

3. **Rede**
```bash
kubectl get svc -n docksaas
kubectl get endpoints -n docksaas
```

### Reset Completo

```bash
# Docker
docker-compose down -v
docker system prune -a

# Kubernetes
kubectl delete namespace docksaas
kubectl apply -f k8s/
```

## ?? Backup e Restore

### Database Backup

```bash
# Docker
docker exec docksaas-postgres pg_dump -U postgres docksaasdb > backup.sql

# Kubernetes
kubectl exec <postgres-pod> -n docksaas -- pg_dump -U postgres docksaasdb > backup.sql
```

### Restore

```bash
# Docker
docker exec -i docksaas-postgres psql -U postgres docksaasdb < backup.sql

# Kubernetes
kubectl exec -i <postgres-pod> -n docksaas -- psql -U postgres docksaasdb < backup.sql
```

## ?? Next Steps

1. Configurar CI/CD pipeline
2. Implementar monitoramento com Prometheus/Grafana
3. Configurar backup automatizado
4. Implementar disaster recovery
5. Configurar observabilidade completa

---

Para suporte adicional, consulte a documentação completa ou abra uma issue no GitHub.