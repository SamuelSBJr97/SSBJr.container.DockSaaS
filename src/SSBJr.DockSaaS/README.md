# DockSaaS - Docker-like SaaS Platform

Uma plataforma SaaS abrangente para gerenciar serviços similares à AWS com arquitetura multi-tenant, cobrança e recursos avançados de monitoramento.

## 🚀 Características Principais

- **Arquitetura Multi-Tenant**: Isolamento completo entre diferentes organizações
- **Serviços AWS-like**: S3 Storage, RDS Database, DynamoDB NoSQL, SQS Queue, Lambda Functions, Apache Kafka
- **Autenticação JWT**: Sistema seguro de autenticação baseado em tokens
- **Interface Blazor**: UI moderna usando MudBlazor e Material Design
- **Monitoramento**: Health checks e métricas em tempo real
- **Orquestração .NET Aspire**: Gerenciamento automático de containers e dependências
- **APIs RESTful**: Endpoints completos para todos os serviços
- **Billing & Cobrança**: Sistema integrado de cobrança por uso

## 🏗️ Tecnologias Utilizadas

- **.NET 8**: Framework principal
- **Blazor Server**: Interface de usuário interativa
- **PostgreSQL**: Banco de dados principal
- **Redis**: Cache e sessões
- **MudBlazor**: Componentes UI Material Design
- **Entity Framework Core**: ORM
- **Serilog**: Logging estruturado
- **.NET Aspire**: Orquestração local
- **JWT Bearer Authentication**: Segurança
- **Swagger/OpenAPI**: Documentação da API

## 📋 Pré-requisitos

- .NET 8 SDK
- Docker Desktop (para Aspire)
- Visual Studio 2022 ou VS Code
- PostgreSQL (gerenciado pelo Aspire)
- Redis (gerenciado pelo Aspire)

## ⚡ Início Rápido

### 1. Clone o Repositório
```bash
git clone https://github.com/SamuelSBJr97/SSBJr.DockSaaS.git
cd SSBJr.DockSaaS/src/SSBJr.DockSaaS
```

### 2. Execute com Aspire (Recomendado)
```bash
dotnet run --project SSBJr.DockSaaS.AppHost
```

### 3. Acesse a Aplicação
- **Web App**: https://localhost:7001
- **API**: https://localhost:7000
- **Aspire Dashboard**: https://localhost:17090
- **Swagger UI**: https://localhost:7000/swagger
- **Health Checks**: https://localhost:7000/health (gerenciado pelo Aspire)

### 4. Login Padrão
- **Email**: admin@docksaas.com
- **Senha**: Admin123!
- **Tenant**: DockSaaS (deixe em branco para usar o padrão)

**⚠️ Troubleshooting do Login:**
Se não conseguir fazer login, tente:
1. Verificar se a API está rodando em `https://localhost:7000`
2. Verificar se o banco de dados PostgreSQL está funcionando
3. Verificar os logs da aplicação para erros de CORS ou conectividade
4. Tentar acessar diretamente: `https://localhost:7000/swagger` para testar a API
5. Verificar health checks em: `https://localhost:7000/health`
6. Limpar o cache do navegador e localStorage
7. **Se ver erro "JSON deserialization error" para health endpoint, isso foi corrigido - atualize o código**
8. **Execute o script de troubleshooting: `.\scripts\troubleshoot-login.ps1`**

**⚠️ Troubleshooting do Dashboard:**
Se encontrar erros no dashboard (blazored-localstorage.js 404, message port closed, JavaScript interop errors):
1. **Estes erros são geralmente inofensivos e não afetam a funcionalidade** - são relacionados ao prerendering do Blazor Server
2. Execute o script de diagnóstico: `.\scripts\fix-dashboard.ps1`
3. Limpe o cache do navegador (Ctrl+F5)
4. Tente navegação privada/incógnita
5. Desative extensões do navegador temporariamente
6. Verifique se as portas estão corretas (Web: 7001, API: 7000)
7. **Se vir "JavaScript interop calls cannot be issued at this time" nos logs, isso é normal durante o prerendering**

**⚠️ Troubleshooting de JSON:**
Se encontrar erros de JSON deserialization:
1. **Health endpoint**: O endpoint `/health` retorna texto/HTML, não JSON - use `CheckHealthAsync()` em vez de `GetAsync<T>()`
2. **API responses**: Verifique se o endpoint realmente retorna JSON válido
3. **Encoding issues**: Certifique-se de que a resposta está em UTF-8

## 🏛️ Arquitetura

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Blazor Web    │────│   API Service   │────│   PostgreSQL    │
│   (Port 7001)   │    │   (Port 7000)   │    │   (Managed)     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
        │                        │                        │
        └────────────────────────┼────────────────────────┘
                                 │
        ┌─────────────────────────┼─────────────────────────┐
        │                        │                         │
    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
    │      Redis      │    │   Apache Kafka  │    │   Kafka UI      │
    │    (Managed)    │    │   (Managed)     │    │   (Optional)    │
    └─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Comunicação de Rede Docker Interna

Todos os serviços (API, Web, PostgreSQL, Redis, Kafka) comunicam-se usando nomes de serviço Docker na rede interna:
- API: `http://apiservice`
- Web: `http://webservice`
- PostgreSQL: `postgres`
- Redis: `redis`
- Kafka: `kafka`

## 📁 Estrutura do Projeto

```
SSBJr.DockSaaS/
├── SSBJr.DockSaaS.AppHost/           # Orquestração Aspire
├── SSBJr.DockSaaS.ApiService/        # API REST
│   ├── Controllers/                  # Controladores da API
│   │   ├── AuthController.cs         # Autenticação
│   │   ├── ServicesController.cs     # Gerenciamento de serviços
│   │   ├── S3StorageController.cs    # Storage S3-like
│   │   ├── RDSDatabaseController.cs  # Database RDS-like
│   │   ├── NoSQLDatabaseController.cs # NoSQL DynamoDB-like
│   │   ├── QueueController.cs        # Queue SQS-like
│   │   ├── LambdaFunctionController.cs # Functions Lambda-like
│   │   ├── KafkaController.cs        # Apache Kafka
│   │   ├── BillingController.cs      # Sistema de cobrança
│   │   └── DashboardController.cs    # Dashboard e métricas
│   ├── Services/                     # Serviços da aplicação
│   ├── Models/                       # Modelos de dados
│   ├── Data/                         # Contexto do banco
│   └── DTOs/                         # Data Transfer Objects
├── SSBJr.DockSaaS.Web/              # Interface Blazor
│   ├── Components/Pages/             # Páginas Blazor
│   ├── Services/                     # Serviços do cliente
│   └── wwwroot/                      # Arquivos estáticos
├── SSBJr.DockSaaS.Tests/            # Testes unitários
├── SSBJr.DockSaaS.ServiceDefaults/  # Configurações padrão
├── docs/                             # Documentação
│   ├── API_EXAMPLES.md               # Exemplos de API
│   ├── DEVELOPER_GUIDE.md            # Guia do desenvolvedor
│   └── DockSaaS.postman_collection.json # Collection Postman
└── scripts/                          # Scripts utilitários
    ├── apply-migrations.ps1          # Aplicar migrações
    └── troubleshoot-login.ps1        # Solução de problemas
```

## ⚙️ Configuração

### Variáveis de Ambiente
- `ASPNETCORE_ENVIRONMENT`: Development/Staging/Production
- `ConnectionStrings__docksaasdb`: String de conexão PostgreSQL
- `ConnectionStrings__kafka`: String de conexão Kafka (gerenciado pelo Aspire)
- `JwtSettings__Secret`: Chave secreta JWT
- `JwtSettings__Issuer`: Emissor do token
- `JwtSettings__Audience`: Audiência do token

### Portas Configuradas
- **API HTTPS**: 7000
- **API HTTP**: 5200
- **Web HTTPS**: 7001
- **Web HTTP**: 5201
- **Aspire Dashboard**: 17090
- **Kafka**: 9092 (interno)
- **Kafka UI**: Acessível via Aspire Dashboard

## 📊 Serviços Disponíveis

### 1. 🗄️ S3-like Storage
**Funcionalidades:**
- Gerenciamento de buckets (criar, listar, deletar)
- Upload/download de objetos
- Versionamento de objetos
- Criptografia server-side
- Controle de acesso público/privado
- Métricas de uso e armazenamento

**APIs Principais:**
- `GET /api/s3storage/{tenant}/{service}/buckets` - Listar buckets
- `POST /api/s3storage/{tenant}/{service}/buckets` - Criar bucket
- `POST /api/s3storage/{tenant}/{service}/buckets/{bucket}/objects` - Upload objeto
- `GET /api/s3storage/{tenant}/{service}/buckets/{bucket}/objects/{key}` - Download objeto

### 2. 🗃️ RDS-like Database
**Funcionalidades:**
- Instâncias PostgreSQL/MySQL gerenciadas
- Execução de queries SQL
- Gerenciamento de tabelas e esquemas
- Backups automáticos e manuais
- Métricas de performance
- Configuração de conexões

**APIs Principais:**
- `GET /api/rdsdatabase/{tenant}/{service}/info` - Informações da instância
- `POST /api/rdsdatabase/{tenant}/{service}/query` - Executar query
- `GET /api/rdsdatabase/{tenant}/{service}/tables` - Listar tabelas
- `POST /api/rdsdatabase/{tenant}/{service}/backups` - Criar backup

### 3. 🧮 DynamoDB-like NoSQL
**Funcionalidades:**
- Tabelas NoSQL com esquema flexível
- Operações CRUD de items
- Query e Scan operations
- Índices secundários globais/locais
- Controle de capacidade (read/write units)
- Escalabilidade automática

**APIs Principais:**
- `GET /api/nosqldatabase/{tenant}/{service}/tables` - Listar tabelas
- `POST /api/nosqldatabase/{tenant}/{service}/tables` - Criar tabela
- `POST /api/nosqldatabase/{tenant}/{service}/tables/{table}/items` - Inserir item
- `POST /api/nosqldatabase/{tenant}/{service}/tables/{table}/query` - Query items

### 4. 📬 SQS-like Queue
**Funcionalidades:**
- Filas FIFO e Standard
- Envio/recebimento de mensagens
- Operações em lote
- Dead letter queues
- Atributos de mensagem
- Retry automático

**APIs Principais:**
- `GET /api/queue/{tenant}/{service}/queues` - Listar filas
- `POST /api/queue/{tenant}/{service}/queues` - Criar fila
- `POST /api/queue/{tenant}/{service}/queues/{queue}/messages` - Enviar mensagem
- `GET /api/queue/{tenant}/{service}/queues/{queue}/messages` - Receber mensagens

### 5. ⚡ Lambda-like Functions
**Funcionalidades:**
- Computação serverless
- Runtime .NET, Python, Node.js
- Invocação síncrona/assíncrona
- Variáveis de ambiente
- Timeout e memória configuráveis
- Métricas de execução

**APIs Principais:**
- `GET /api/function/{tenant}/{service}/functions` - Listar funções
- `POST /api/function/{tenant}/{service}/functions` - Criar função
- `POST /api/function/{tenant}/{service}/functions/{function}/invoke` - Invocar função
- `PUT /api/function/{tenant}/{service}/functions/{function}/code` - Atualizar código

### 6. 🔄 Apache Kafka (Gerenciado pelo Aspire)
**Funcionalidades:**
- Plataforma de streaming distribuída totalmente gerenciada
- Gerenciamento automático de tópicos e partições
- Produção/consumo de mensagens via API
- Schema Registry integrado (opcional)
- Kafka Connect (opcional)
- Monitoramento de saúde do cluster
- **Interface Kafka UI** para visualização via Aspire Dashboard

**APIs Principais:**
- `GET /api/kafka/{tenant}/{service}/cluster/info` - Informações do cluster
- `GET /api/kafka/{tenant}/{service}/topics` - Listar tópicos
- `POST /api/kafka/{tenant}/{service}/topics` - Criar tópico
- `POST /api/kafka/{tenant}/{service}/topics/{topic}/messages` - Produzir mensagem
- `GET /api/kafka/{tenant}/{service}/topics/{topic}/messages` - Consumir mensagens
- `GET /api/kafka/{tenant}/{service}/health` - Status de saúde do Kafka

**Configurações Avançadas:**
- Configuração automática pelo Aspire
- Volumes persistentes de dados
- Interface web Kafka UI incluída
- Conexão segura via service discovery

## 👥 Gerenciamento de Usuários

### Roles Disponíveis
- **Admin**: Acesso completo ao sistema
- **Manager**: Gerenciamento de usuários e serviços
- **User**: Acesso básico aos serviços

### Multi-Tenancy
- Isolamento completo entre tenants
- Configurações por tenant
- Cobrança separada
- Limites de recursos por plano

### Planos de Serviço
- **Free Plan**: 1.000 requests/hora, recursos limitados
- **Pro Plan**: 10.000 requests/hora, recursos expandidos
- **Enterprise Plan**: 100.000 requests/hora, recursos ilimitados

## 🔐 Segurança

### Autenticação
- JWT tokens seguros com refresh tokens
- Expiração configurável
- Suporte a múltiplos tenants
- API keys por instância de serviço

### Autorização
- Role-based access control (RBAC)
- Políticas de acesso por tenant
- Validação de API keys
- Controle de rate limiting

### Conformidade
- Logs de auditoria completos
- Criptografia em trânsito (HTTPS)
- Criptografia em repouso (opcional)
- Isolamento de dados por tenant

## 📈 Monitoramento

### Health Checks
- Status da aplicação em `/health` (gerenciado pelo Aspire)
- Conectividade do banco
- Status dos serviços
- Disponibilidade da API

### Logging
- Logs estruturados com Serilog
- Níveis configuráveis
- Correlação de requests
- Métricas de performance

### Métricas
- Uso de recursos por serviço
- Performance de APIs
- Billing e custos
- Alertas de limite

## 💰 Sistema de Billing

### Recursos de Cobrança
- Cobrança por uso de recursos
- Métricas de consumo em tempo real
- Relatórios de billing
- Alertas de limite de gasto
- Faturamento automático

### Métricas Rastreadas
- Armazenamento S3 (GB)
- Operações de banco (queries)
- Mensagens em fila (count)
- Execuções Lambda (invocations)
- Throughput Kafka (messages/sec)

## 🚀 Deploy

### Desenvolvimento Local
```bash
# Com Aspire (recomendado)
dotnet run --project SSBJr.DockSaaS.AppHost

# Sem Aspire
dotnet run --project SSBJr.DockSaaS.ApiService
dotnet run --project SSBJr.DockSaaS.Web
```

### Produção
- Configure variáveis de ambiente
- Use HTTPS obrigatório
- Configure conexões de banco seguras
- Altere chaves JWT padrão
- Configure rate limiting
- Implemente backup automático

### Docker Compose (Alternativo)
```bash
docker-compose up -d
```

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Testes de integração
dotnet test SSBJr.DockSaaS.Tests
```

## 📚 Documentação

### Para Desenvolvedores
- **[API Examples](docs/API_EXAMPLES.md)**: Exemplos completos de todas as APIs
- **[Developer Guide](docs/DEVELOPER_GUIDE.md)**: Guia rápido para desenvolvedores
- **[Postman Collection](docs/DockSaaS.postman_collection.json)**: Collection para testes

### APIs e Swagger
- **Swagger UI**: https://localhost:7000/swagger
- **OpenAPI Spec**: https://localhost:7000/swagger/v1/swagger.json

### Scripts Utilitários
- `scripts/apply-migrations.ps1`: Aplicar migrações do banco
- `scripts/troubleshoot-login.ps1`: Diagnóstico de problemas
- `scripts/test-api-health.ps1`: Teste de conectividade da API
- `scripts/test-kafka-integration.ps1`: Teste de integração Kafka

## 🔧 Requisitos Técnicos

### Funcionais
✅ **Autenticação e Autorização**
- Sistema JWT com refresh tokens
- Multi-tenant com isolamento completo
- RBAC com roles Admin/Manager/User
- API keys por instância de serviço

✅ **Serviços AWS-like**
- S3 Storage com buckets e objetos
- RDS Database com queries SQL
- NoSQL Database tipo DynamoDB
- SQS Queue com mensagens
- Lambda Functions serverless
- Apache Kafka streaming

✅ **Interface Web**
- Dashboard Blazor responsivo
- Gerenciamento de serviços
- Monitoramento em tempo real
- Sistema de billing
- **Prerendering-safe**: Lida graciosamente com limitações do JavaScript interop durante SSR

✅ **APIs RESTful**
- Endpoints completos para todos os serviços
- Documentação Swagger/OpenAPI
- Rate limiting por plano
- Tratamento de erros padronizado

### Não-Funcionais
✅ **Performance**
- Resposta < 500ms para 95% das requests
- Suporte a 1000+ usuários simultâneos
- Cache Redis para sessões
- Otimização de queries

✅ **Escalabilidade**
- Arquitetura multi-tenant
- Horizontal scaling ready
- Load balancing support
- Database connection pooling

✅ **Confiabilidade**
- Health checks automatizados (via Aspire)
- Retry automático para falhas
- Logs estruturados
- Backup automático

✅ **Segurança**
- HTTPS obrigatório
- Criptografia de dados
- Validação de entrada
- Logs de auditoria

✅ **Observabilidade**
- Métricas em tempo real
- Dashboards de monitoramento
- Alertas configuráveis
- Trace de requests

## 🛠️ Scripts e Ferramentas

### PowerShell Scripts
```powershell
# Aplicar migrações
.\scripts\apply-migrations.ps1

# Diagnóstico de problemas
.\scripts\troubleshoot-login.ps1

# Teste de conectividade da API
.\scripts\test-api-health.ps1

# Teste de integração Kafka
.\scripts\test-kafka-integration.ps1
```

### Postman Testing
```bash
# Importar collection
Import: docs/DockSaaS.postman_collection.json

# Configurar variáveis
base_url: https://localhost:7000
jwt_token: (obtido no login)
tenant_id: (obtido no login)
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

### Padrões de Código
- Use .NET 8 conventions
- Implemente testes unitários
- Documente APIs com XML comments
- Siga o padrão de logging estruturado

## 📄 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

## 👨‍💻 Autor

**Samuel da Silva B Jr**
- GitHub: [@SamuelSBJr97](https://github.com/SamuelSBJr97)
- Email: samuel@example.com
- LinkedIn: [samuel-silva-jr](https://linkedin.com/in/samuel-silva-jr)

## 🙏 Agradecimentos

- Microsoft pela plataforma .NET e Aspire
- Comunidade MudBlazor pelos componentes UI
- Comunidade open source pelas bibliotecas utilizadas
- AWS pela inspiração dos serviços

## 📞 Suporte

### Problemas Comuns
1. **Erro de conexão**: Verifique se o Aspire está rodando
2. **Login falha**: Execute o troubleshooting script
3. **API não responde**: Verifique health check em `/health`
4. **Performance**: Monitore no dashboard Aspire

### Recursos de Ajuda
- **Issues**: [GitHub Issues](https://github.com/SamuelSBJr97/SSBJr.DockSaaS/issues)
- **Discussões**: [GitHub Discussions](https://github.com/SamuelSBJr97/SSBJr.DockSaaS/discussions)
- **Wiki**: [GitHub Wiki](https://github.com/SamuelSBJr97/SSBJr.DockSaaS/wiki)

---

⭐ **Se este projeto foi útil para você, considere dar uma estrela!**

📈 **Status do Build**: ![Build Status](https://github.com/SamuelSBJr97/SSBJr.DockSaaS/workflows/CI/badge.svg)

🔧 **Versão**: 1.0.0 | 📅 **Última Atualização**: Janeiro 2024