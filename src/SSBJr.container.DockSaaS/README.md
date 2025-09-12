# DockSaaS - Docker-like SaaS Platform

Uma plataforma SaaS abrangente para gerenciar serviços similares à AWS com arquitetura multi-tenant, cobrança e recursos avançados de monitoramento.

## ?? Características Principais

- **Arquitetura Multi-Tenant**: Isolamento completo entre diferentes organizações
- **Serviços AWS-like**: S3 Storage, RDS Database, DynamoDB NoSQL, SQS Queue, Lambda Functions
- **Autenticação JWT**: Sistema seguro de autenticação baseado em tokens
- **Interface Blazor**: UI moderna usando MudBlazor e Material Design
- **Monitoramento**: Health checks e métricas em tempo real
- **Orquestração .NET Aspire**: Gerenciamento automático de containers e dependências

## ??? Tecnologias Utilizadas

- **.NET 8**: Framework principal
- **Blazor Server**: Interface de usuário interativa
- **PostgreSQL**: Banco de dados principal
- **Redis**: Cache e sessões
- **MudBlazor**: Componentes UI Material Design
- **Entity Framework Core**: ORM
- **Serilog**: Logging estruturado
- **.NET Aspire**: Orquestração local

## ?? Pré-requisitos

- .NET 8 SDK
- Docker Desktop (para Aspire)
- Visual Studio 2022 ou VS Code

## ?? Início Rápido

### 1. Clone o Repositório
```bash
git clone https://github.com/SamuelSBJr97/SSBJr.container.DockSaaS.git
cd SSBJr.container.DockSaaS/src/SSBjr.container.DockSaaS
```

### 2. Execute com Aspire (Recomendado)
```bash
dotnet run --project SSBJr.container.DockSaaS.AppHost
```

### 3. Acesse a Aplicação
- **Web App**: https://localhost:7001
- **API**: https://localhost:7000
- **Aspire Dashboard**: https://localhost:17090

### 4. Login Padrão
- **Email**: admin@docksaas.com
- **Senha**: Admin123!
- **Tenant**: DockSaaS

## ??? Arquitetura

```
???????????????????    ???????????????????    ???????????????????
?   Blazor Web    ??????   API Service   ??????   PostgreSQL    ?
?   (Port 7001)   ?    ?   (Port 7000)   ?    ?   (Managed)     ?
???????????????????    ???????????????????    ???????????????????
        ?                        ?                        ?
        ???????????????????????????????????????????????????
                                 ?
                    ???????????????????
                    ?      Redis      ?
                    ?    (Managed)    ?
                    ???????????????????
```

## ?? Estrutura do Projeto

```
SSBJr.container.DockSaaS/
??? SSBJr.container.DockSaaS.AppHost/     # Orquestração Aspire
??? SSBJr.container.DockSaaS.ApiService/  # API REST
??? SSBJr.container.DockSaaS.Web/         # Interface Blazor
??? SSBJr.container.DockSaaS.Tests/       # Testes unitários
??? SSBJr.container.DockSaaS.ServiceDefaults/ # Configurações padrão
```

## ?? Configuração

### Variáveis de Ambiente
- `ASPNETCORE_ENVIRONMENT`: Development/Staging/Production
- `ConnectionStrings__docksaasdb`: String de conexão PostgreSQL
- `JwtSettings__Secret`: Chave secreta JWT

### Portas Configuradas
- **API HTTPS**: 7000
- **API HTTP**: 5200
- **Web HTTPS**: 7001
- **Web HTTP**: 5201
- **Aspire Dashboard**: 17090

# SSBjr.saas.DockSaaS

## Rede Docker Interna

Todos os serviços (API, Web, PostgreSQL, Redis) comunicam-se usando nomes de serviço Docker na rede interna:
- API: `http://apiservice`
- Web: `http://webservice`
- PostgreSQL: `postgres`
- Redis: `redis`

Não há portas fixas expostas. Para acessar externamente, utilize o dashboard Aspire ou configure endpoints públicos conforme necessário.

## Como funciona
- Aspire orquestra todos os containers na mesma rede.
- As configurações de conexão e URLs usam nomes de serviço Docker.
- Comunicação entre serviços é automática e segura na rede interna.

## Exemplo de configuração

**API appsettings.json:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=postgres;Database=docksaasdb;Username=postgres;Password=postgres;",
  "Redis": "redis:6379"
},
"ServiceEndpoints": {
  "BaseUrl": "http://apiservice"
},
"BlazorClientUrls": "http://webservice"
```

**Web appsettings.Development.json:**
```json
"ApiBaseUrl": "http://apiservice"
```

**AppHost.cs:**
```csharp
var apiService = builder.AddProject<...>("apiservice")
    .WithReference(docksaasdb)
    .WithReference(redis)
    .WithEnvironment("ServiceEndpoints__BaseUrl", "http://apiservice");
```

## Vantagens
- Portabilidade
- Escalabilidade
- Segurança
- Sem conflitos de porta

## ?? Serviços Disponíveis

### 1. S3-like Storage
- Buckets e objetos
- Versionamento
- Criptografia
- Controle de acesso

### 2. RDS Database
- PostgreSQL e MySQL
- Backups automáticos
- Conexões gerenciadas

### 3. NoSQL Database
- Esquema flexível
- Escalabilidade automática
- Capacidade configurável

### 4. Message Queue
- Filas FIFO e Standard
- Retry automático
- Dead letter queues

### 5. Serverless Functions
- Runtime .NET, Python, Node.js
- Timeout configurável
- Memória ajustável

## ?? Gerenciamento de Usuários

### Roles Disponíveis
- **Admin**: Acesso completo ao sistema
- **Manager**: Gerenciamento de usuários e serviços
- **User**: Acesso básico aos serviços

### Multi-Tenancy
- Isolamento completo entre tenants
- Configurações por tenant
- Cobrança separada

## ?? Segurança

### Autenticação
- JWT tokens seguros
- Refresh tokens
- Expiração configurável

### Autorização
- Role-based access control
- Políticas de acesso
- API keys por serviço

## ?? Monitoramento

### Health Checks
- Status da aplicação
- Conectividade do banco
- Status dos serviços

### Logging
- Logs estruturados com Serilog
- Níveis configuráveis
- Correlação de requests

## ?? Deploy

### Desenvolvimento Local
```bash
# Com Aspire (recomendado)
dotnet run --project SSBJr.container.DockSaaS.AppHost

# Sem Aspire
dotnet run --project SSBJr.container.DockSaaS.ApiService
dotnet run --project SSBJr.container.DockSaaS.Web
```

### Produção
- Configure variáveis de ambiente
- Use HTTPS obrigatório
- Configure conexões de banco seguras
- Altere chaves JWT padrão

## ?? Testes

```bash
# Executar todos os testes
dotnet test

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## ?? Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## ?? Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

## ????? Autor

**Samuel da Silva B Jr**
- GitHub: [@SamuelSBJr97](https://github.com/SamuelSBJr97)
- Email: samuel@example.com

## ?? Agradecimentos

- Microsoft pela plataforma .NET e Aspire
- Comunidade MudBlazor pelos componentes UI
- Comunidade open source pelas bibliotecas utilizadas

---

? Se este projeto foi útil para você, considere dar uma estrela!