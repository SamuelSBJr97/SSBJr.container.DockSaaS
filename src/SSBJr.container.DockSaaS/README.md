# DockSaaS - AWS-like SaaS Platform

![DockSaaS](https://img.shields.io/badge/DockSaaS-v1.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Docker](https://img.shields.io/badge/Docker-Ready-blue)
![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-blue)
![License](https://img.shields.io/badge/License-MIT-green)

DockSaaS � uma plataforma SaaS completa que permite aos administradores criar e gerenciar servi�os similares aos da AWS de forma din�mica e multi-tenant. A plataforma oferece provisionamento autom�tico, cobran�a baseada em uso, monitoramento em tempo real e muito mais.

## ?? Caracter�sticas Principais

### ? **Arquitetura Multi-Tenant Completa**
- Isolamento completo de dados entre tenants
- Planos configur�veis (Free, Pro, Enterprise)
- Gest�o de quotas e limites por tenant
- Branding personalizado por organiza��o

### ? **Servi�os AWS-like**
- **S3-like Storage**: Armazenamento de objetos com buckets, versionamento e criptografia
- **RDS-like Database**: Bancos relacionais gerenciados com backup autom�tico
- **DynamoDB-like NoSQL**: Banco NoSQL com capacidade configur�vel
- **SQS-like Queue**: Sistema de filas para desacoplamento de aplica��es
- **Lambda-like Functions**: Computa��o serverless para execu��o de c�digo
- **CloudWatch-like Monitoring**: Monitoramento e m�tricas em tempo real

### ? **Sistema de Cobran�a Avan�ado**
- Cobran�a baseada em uso real
- Tr�s tiers de pre�o (Free, Pro, Enterprise)
- Alertas autom�ticos de quota (80%, 95%)
- Hist�rico detalhado de uso (90 dias)
- Previs�o de custos mensais

### ? **Interface Moderna**
- Blazor Server com MudBlazor
- Dashboard em tempo real
- Gerenciamento completo de servi�os
- Administra��o de usu�rios e permiss�es
- Logs de auditoria com filtros avan�ados

### ? **Pronto para Produ��o**
- Docker e Kubernetes ready
- Health checks integrados
- Logging estruturado com Serilog
- Background services para m�tricas
- CI/CD ready com scripts automatizados

## ?? Pr�-requisitos

### Para Desenvolvimento
- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 ou VS Code
- 4GB RAM
- 10GB espa�o em disco

### Para Produ��o
- Docker e Docker Compose
- Kubernetes (opcional)
- PostgreSQL 15+
- Redis 7+
- 8GB RAM recomendado
- 20GB espa�o em disco

## ?? Quick Start

### 1. **Clone o Reposit�rio**
```bash
git clone https://github.com/SamuelSBJr97/SSBJr.container.DockSaaS.git
cd SSBJr.container.DockSaaS
```

### 2. **Desenvolvimento R�pido (.NET Aspire)**
```bash
# Linux/Mac
chmod +x deploy.sh
./deploy.sh dev

# Windows
deploy.bat dev

# Executar aplica��o
dotnet run --project SSBJr.container.DockSaaS.AppHost
```

**URLs de Desenvolvimento:**
- ?? **Aspire Dashboard**: https://localhost:15888
- ?? **Web Interface**: https://localhost:7001
- ?? **API Swagger**: https://localhost:7000/swagger
- ?? **pgAdmin**: http://localhost:8080

### 3. **Produ��o Docker**
```bash
# Build e Deploy completo
./deploy.sh prod

# OU apenas build
./deploy.sh build
docker-compose up -d
```

**URLs de Produ��o:**
- ?? **Web Interface**: http://localhost:3000
- ?? **API Swagger**: http://localhost:5000/swagger
- ?? **pgAdmin**: http://localhost:8080
- ?? **Redis Commander**: http://localhost:8081

### 4. **Deploy Kubernetes**
```bash
# Deploy completo no K8s
./deploy.sh k8s

# Adicionar ao hosts file
echo "127.0.0.1 docksaas.local" | sudo tee -a /etc/hosts
```

**URLs Kubernetes:**
- ?? **Web Interface**: http://docksaas.local
- ?? **API Swagger**: http://docksaas.local/api/swagger

## ??? Comandos �teis

### **Script de Deploy Unificado**
```bash
# Verificar depend�ncias
./deploy.sh check

# Limpar ambiente
./deploy.sh clean

# Build completo
./deploy.sh build

# Status dos servi�os
./deploy.sh status

# Ver logs
./deploy.sh logs          # Todos os servi�os
./deploy.sh logs api      # Apenas API

# Parar tudo
./deploy.sh stop

# Ajuda
./deploy.sh help
```

### **Docker Compose Manual**
```bash
# Desenvolvimento
docker-compose -f docker-compose.dev.yml up -d

# Produ��o
docker-compose up -d

# Logs
docker-compose logs -f
docker-compose logs -f docksaas-api

# Parar
docker-compose down
```

### **Kubernetes Manual**
```bash
# Deploy
kubectl apply -f k8s/infrastructure.yaml
kubectl apply -f k8s/applications.yaml

# Status
kubectl get pods -n docksaas
kubectl get services -n docksaas

# Logs
kubectl logs -f deployment/docksaas-api -n docksaas

# Limpar
kubectl delete namespace docksaas
```

## ??? Arquitetura

### **Componentes Principais**
```
???????????????????    ???????????????????    ???????????????????
?   Blazor Web    ?    ?   API Service   ?    ?   PostgreSQL    ?
?   (Port 7001)   ??????   (Port 7000)   ??????   (Port 5432)   ?
???????????????????    ???????????????????    ???????????????????
         ?                       ?                       ?
         ?????????????????????????????????????????????????
                                 ?
                    ???????????????????
                    ?      Redis      ?
                    ?   (Port 6379)   ?
                    ???????????????????
```

### **Tecnologias Utilizadas**
- **Backend**: ASP.NET Core 8, Entity Framework Core, PostgreSQL
- **Frontend**: Blazor Server, MudBlazor
- **Cache**: Redis
- **Orquestra��o**: .NET Aspire
- **Containeriza��o**: Docker, Kubernetes
- **Monitoramento**: Health Checks, Serilog
- **Testes**: XUnit, MSTest

## ?? Funcionalidades Detalhadas

### **1. Gerenciamento de Servi�os**
- Cria��o din�mica de inst�ncias de servi�o
- Configura��o via JSON Schema
- Start/Stop de servi�os
- Monitoramento de sa�de
- Backup e restore

### **2. Sistema de Cobran�a**
- Planos: Free (1GB), Pro (100GB), Enterprise (1TB)
- M�tricas: Storage, API calls, DB queries, Functions, Queue messages
- Alertas autom�ticos de quota
- Relat�rios de uso

### **3. Multi-Tenant**
- Isolamento completo de dados
- Configura��es por tenant
- Convites de usu�rios
- Roles: Admin, Manager, User

### **4. API Gateway**
- Endpoints din�micos por servi�o
- Autentica��o JWT + API Keys
- Rate limiting
- Documenta��o Swagger autom�tica

### **5. Monitoramento**
- M�tricas em tempo real
- Logs de auditoria
- Health checks
- Alertas configur�veis

## ?? Seguran�a

### **Autentica��o e Autoriza��o**
- JWT tokens com refresh
- Role-based access control (RBAC)
- API Keys com scoping
- IP whitelisting

### **Isolamento de Dados**
- Schema separation no PostgreSQL
- Tenant-specific API keys
- Audit trail completo

### **Produ��o Security**
- HTTPS obrigat�rio
- Secrets management
- Non-root containers
- Network policies (K8s)

## ?? Monitoramento e Observabilidade

### **Health Checks**
```bash
# API Health
curl http://localhost:5000/health

# Web Health  
curl http://localhost:3000/health

# Detailed JSON response
curl -H "Accept: application/json" http://localhost:5000/health
```

### **M�tricas Dispon�veis**
- CPU, Memory, Storage usage
- API request rates
- Database connections
- Queue message counts
- Function invocations

### **Logs Estruturados**
```bash
# API logs
docker logs docksaas-api -f

# Web logs
docker logs docksaas-web -f

# Structured JSON logs
docker logs docksaas-api --since=1h | jq .
```

## ?? Testes

### **Executar Testes**
```bash
# Todos os testes
dotnet test

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Espec�fico
dotnet test SSBJr.container.DockSaaS.Tests
```

### **Testes Inclu�dos**
- Unit tests para services
- Integration tests com Aspire
- API endpoint tests
- Database tests

## ?? Configura��o

### **Vari�veis de Ambiente**

#### **API Service**
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=postgres;Database=docksaasdb;Username=postgres;Password=postgres
ConnectionStrings__Redis=redis:6379
JwtSettings__Secret=your-secret-key
JwtSettings__Issuer=DockSaaS
JwtSettings__Audience=DockSaaSUsers
ServiceEndpoints__BaseUrl=http://api:8080
```

#### **Web Service**
```env
ASPNETCORE_ENVIRONMENT=Production
ApiBaseUrl=http://api:8080
ConnectionStrings__Redis=redis:6379
```

### **Customiza��o**

#### **Adicionar Novo Tipo de Servi�o**
1. Criar provider em `Services/ServiceProviders.cs`
2. Implementar `IServiceProvider`
3. Registrar no DI container
4. Adicionar configura��o no seeder

#### **Modificar Planos de Cobran�a**
1. Editar `BillingService.cs`
2. Atualizar `InitializePricingTiers()`
3. Executar migra��o se necess�rio

## ?? Documenta��o da API

### **Endpoints Principais**

#### **Autentica��o**
```bash
POST /api/auth/register  # Registrar usu�rio
POST /api/auth/login     # Login
POST /api/auth/refresh   # Refresh token
```

#### **Servi�os**
```bash
GET    /api/services              # Listar servi�os
POST   /api/services              # Criar servi�o
GET    /api/services/{id}         # Detalhes do servi�o
PUT    /api/services/{id}         # Atualizar servi�o
DELETE /api/services/{id}         # Deletar servi�o
POST   /api/services/{id}/start   # Iniciar servi�o
POST   /api/services/{id}/stop    # Parar servi�o
```

#### **Cobran�a**
```bash
GET /api/billing/usage         # Uso atual
GET /api/billing/quotas        # Quotas do tenant
GET /api/billing/alerts        # Alertas de cobran�a
GET /api/billing/bill/{y}/{m}  # Fatura mensal
```

### **Documenta��o Swagger**
Acesse `/swagger` em qualquer ambiente para documenta��o interativa completa.

## ?? Roadmap

### **Pr�ximas Funcionalidades**
- [ ] GraphQL API
- [ ] Mobile app (Blazor Hybrid)
- [ ] Advanced analytics
- [ ] Marketplace de templates
- [ ] Multi-region deployment
- [ ] CDN integration

### **Melhorias de Infraestrutura**
- [ ] Prometheus/Grafana monitoring
- [ ] ELK stack para logs
- [ ] GitOps com ArgoCD
- [ ] Disaster recovery automation
- [ ] Performance optimizations

## ?? Contribuindo

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudan�as (`git commit -m 'Add AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## ?? License

Distribu�do sob a licen�a MIT. Veja `LICENSE` para mais informa��es.

## ?? Suporte

- **Issues**: [GitHub Issues](https://github.com/SamuelSBJr97/SSBJr.container.DockSaaS/issues)
- **Discussions**: [GitHub Discussions](https://github.com/SamuelSBJr97/SSBJr.container.DockSaaS/discussions)
- **Email**: samuel@docksaas.com

---

**DockSaaS** - Construindo o futuro das plataformas SaaS ??

Made with ?? by [Samuel da Silva B Jr](https://github.com/SamuelSBJr97)