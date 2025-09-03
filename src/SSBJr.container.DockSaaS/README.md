# DockSaaS - AWS-like SaaS Platform

![DockSaaS](https://img.shields.io/badge/DockSaaS-v1.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Docker](https://img.shields.io/badge/Docker-Ready-blue)
![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-blue)
![License](https://img.shields.io/badge/License-MIT-green)

DockSaaS é uma plataforma SaaS completa que permite aos administradores criar e gerenciar serviços similares aos da AWS de forma dinâmica e multi-tenant. A plataforma oferece provisionamento automático, cobrança baseada em uso, monitoramento em tempo real e muito mais.

## ?? Características Principais

### ? **Arquitetura Multi-Tenant Completa**
- Isolamento completo de dados entre tenants
- Planos configuráveis (Free, Pro, Enterprise)
- Gestão de quotas e limites por tenant
- Branding personalizado por organização

### ? **Serviços AWS-like**
- **S3-like Storage**: Armazenamento de objetos com buckets, versionamento e criptografia
- **RDS-like Database**: Bancos relacionais gerenciados com backup automático
- **DynamoDB-like NoSQL**: Banco NoSQL com capacidade configurável
- **SQS-like Queue**: Sistema de filas para desacoplamento de aplicações
- **Lambda-like Functions**: Computação serverless para execução de código
- **CloudWatch-like Monitoring**: Monitoramento e métricas em tempo real

### ? **Sistema de Cobrança Avançado**
- Cobrança baseada em uso real
- Três tiers de preço (Free, Pro, Enterprise)
- Alertas automáticos de quota (80%, 95%)
- Histórico detalhado de uso (90 dias)
- Previsão de custos mensais

### ? **Interface Moderna**
- Blazor Server com MudBlazor
- Dashboard em tempo real
- Gerenciamento completo de serviços
- Administração de usuários e permissões
- Logs de auditoria com filtros avançados

### ? **Pronto para Produção**
- Docker e Kubernetes ready
- Health checks integrados
- Logging estruturado com Serilog
- Background services para métricas
- CI/CD ready com scripts automatizados

## ?? Pré-requisitos

### Para Desenvolvimento
- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 ou VS Code
- 4GB RAM
- 10GB espaço em disco

### Para Produção
- Docker e Docker Compose
- Kubernetes (opcional)
- PostgreSQL 15+
- Redis 7+
- 8GB RAM recomendado
- 20GB espaço em disco

## ?? Quick Start

### 1. **Clone o Repositório**
```bash
git clone https://github.com/SamuelSBJr97/SSBJr.container.DockSaaS.git
cd SSBJr.container.DockSaaS
```

### 2. **Desenvolvimento Rápido (.NET Aspire)**
```bash
# Linux/Mac
chmod +x deploy.sh
./deploy.sh dev

# Windows
deploy.bat dev

# Executar aplicação
dotnet run --project SSBJr.container.DockSaaS.AppHost
```

**URLs de Desenvolvimento:**
- ?? **Aspire Dashboard**: https://localhost:15888
- ?? **Web Interface**: https://localhost:7001
- ?? **API Swagger**: https://localhost:7000/swagger
- ?? **pgAdmin**: http://localhost:8080

### 3. **Produção Docker**
```bash
# Build e Deploy completo
./deploy.sh prod

# OU apenas build
./deploy.sh build
docker-compose up -d
```

**URLs de Produção:**
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

## ??? Comandos Úteis

### **Script de Deploy Unificado**
```bash
# Verificar dependências
./deploy.sh check

# Limpar ambiente
./deploy.sh clean

# Build completo
./deploy.sh build

# Status dos serviços
./deploy.sh status

# Ver logs
./deploy.sh logs          # Todos os serviços
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

# Produção
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
- **Orquestração**: .NET Aspire
- **Containerização**: Docker, Kubernetes
- **Monitoramento**: Health Checks, Serilog
- **Testes**: XUnit, MSTest

## ?? Funcionalidades Detalhadas

### **1. Gerenciamento de Serviços**
- Criação dinâmica de instâncias de serviço
- Configuração via JSON Schema
- Start/Stop de serviços
- Monitoramento de saúde
- Backup e restore

### **2. Sistema de Cobrança**
- Planos: Free (1GB), Pro (100GB), Enterprise (1TB)
- Métricas: Storage, API calls, DB queries, Functions, Queue messages
- Alertas automáticos de quota
- Relatórios de uso

### **3. Multi-Tenant**
- Isolamento completo de dados
- Configurações por tenant
- Convites de usuários
- Roles: Admin, Manager, User

### **4. API Gateway**
- Endpoints dinâmicos por serviço
- Autenticação JWT + API Keys
- Rate limiting
- Documentação Swagger automática

### **5. Monitoramento**
- Métricas em tempo real
- Logs de auditoria
- Health checks
- Alertas configuráveis

## ?? Segurança

### **Autenticação e Autorização**
- JWT tokens com refresh
- Role-based access control (RBAC)
- API Keys com scoping
- IP whitelisting

### **Isolamento de Dados**
- Schema separation no PostgreSQL
- Tenant-specific API keys
- Audit trail completo

### **Produção Security**
- HTTPS obrigatório
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

### **Métricas Disponíveis**
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

# Específico
dotnet test SSBJr.container.DockSaaS.Tests
```

### **Testes Incluídos**
- Unit tests para services
- Integration tests com Aspire
- API endpoint tests
- Database tests

## ?? Configuração

### **Variáveis de Ambiente**

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

### **Customização**

#### **Adicionar Novo Tipo de Serviço**
1. Criar provider em `Services/ServiceProviders.cs`
2. Implementar `IServiceProvider`
3. Registrar no DI container
4. Adicionar configuração no seeder

#### **Modificar Planos de Cobrança**
1. Editar `BillingService.cs`
2. Atualizar `InitializePricingTiers()`
3. Executar migração se necessário

## ?? Documentação da API

### **Endpoints Principais**

#### **Autenticação**
```bash
POST /api/auth/register  # Registrar usuário
POST /api/auth/login     # Login
POST /api/auth/refresh   # Refresh token
```

#### **Serviços**
```bash
GET    /api/services              # Listar serviços
POST   /api/services              # Criar serviço
GET    /api/services/{id}         # Detalhes do serviço
PUT    /api/services/{id}         # Atualizar serviço
DELETE /api/services/{id}         # Deletar serviço
POST   /api/services/{id}/start   # Iniciar serviço
POST   /api/services/{id}/stop    # Parar serviço
```

#### **Cobrança**
```bash
GET /api/billing/usage         # Uso atual
GET /api/billing/quotas        # Quotas do tenant
GET /api/billing/alerts        # Alertas de cobrança
GET /api/billing/bill/{y}/{m}  # Fatura mensal
```

### **Documentação Swagger**
Acesse `/swagger` em qualquer ambiente para documentação interativa completa.

## ?? Roadmap

### **Próximas Funcionalidades**
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
3. Commit suas mudanças (`git commit -m 'Add AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## ?? License

Distribuído sob a licença MIT. Veja `LICENSE` para mais informações.

## ?? Suporte

- **Issues**: [GitHub Issues](https://github.com/SamuelSBJr97/SSBJr.container.DockSaaS/issues)
- **Discussions**: [GitHub Discussions](https://github.com/SamuelSBJr97/SSBJr.container.DockSaaS/discussions)
- **Email**: samuel@docksaas.com

---

**DockSaaS** - Construindo o futuro das plataformas SaaS ??

Made with ?? by [Samuel da Silva B Jr](https://github.com/SamuelSBJr97)