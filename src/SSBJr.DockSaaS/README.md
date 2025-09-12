# DockSaaS - Docker-like SaaS Platform

Uma plataforma SaaS abrangente para gerenciar servi�os similares � AWS com arquitetura multi-tenant, cobran�a e recursos avan�ados de monitoramento.

## ?? Caracter�sticas Principais

- **Arquitetura Multi-Tenant**: Isolamento completo entre diferentes organiza��es
- **Servi�os AWS-like**: S3 Storage, RDS Database, DynamoDB NoSQL, SQS Queue, Lambda Functions
- **Autentica��o JWT**: Sistema seguro de autentica��o baseado em tokens
- **Interface Blazor**: UI moderna usando MudBlazor e Material Design
- **Monitoramento**: Health checks e m�tricas em tempo real
- **Orquestra��o .NET Aspire**: Gerenciamento autom�tico de containers e depend�ncias

## ??? Tecnologias Utilizadas

- **.NET 8**: Framework principal
- **Blazor Server**: Interface de usu�rio interativa
- **PostgreSQL**: Banco de dados principal
- **Redis**: Cache e sess�es
- **MudBlazor**: Componentes UI Material Design
- **Entity Framework Core**: ORM
- **Serilog**: Logging estruturado
- **.NET Aspire**: Orquestra��o local

## ?? Pr�-requisitos

- .NET 8 SDK
- Docker Desktop (para Aspire)
- Visual Studio 2022 ou VS Code

## ?? In�cio R�pido

### 1. Clone o Reposit�rio
```bash
git clone https://github.com/SamuelSBJr97/SSBJr.DockSaaS.git
cd SSBJr.DockSaaS/src/SSBJr.DockSaaS
```

### 2. Execute com Aspire (Recomendado)
```bash
dotnet run --project SSBJr.DockSaaS.AppHost
```

### 3. Acesse a Aplica��o
- **Web App**: https://localhost:7001
- **API**: https://localhost:7000
- **Aspire Dashboard**: https://localhost:17090

### 4. Login Padr�o
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
SSBJr.DockSaaS/
??? SSBJr.DockSaaS.AppHost/     # Orquestra��o Aspire
??? SSBJr.DockSaaS.ApiService/  # API REST
??? SSBJr.DockSaaS.Web/         # Interface Blazor
??? SSBJr.DockSaaS.Tests/       # Testes unit�rios
??? SSBJr.DockSaaS.ServiceDefaults/ # Configura��es padr�o
```

## ?? Configura��o

### Vari�veis de Ambiente
- `ASPNETCORE_ENVIRONMENT`: Development/Staging/Production
- `ConnectionStrings__docksaasdb`: String de conex�o PostgreSQL
- `JwtSettings__Secret`: Chave secreta JWT

### Portas Configuradas
- **API HTTPS**: 7000
- **API HTTP**: 5200
- **Web HTTPS**: 7001
- **Web HTTP**: 5201
- **Aspire Dashboard**: 17090

# SSBjr.saas.DockSaaS

## Rede Docker Interna

Todos os servi�os (API, Web, PostgreSQL, Redis) comunicam-se usando nomes de servi�o Docker na rede interna:
- API: `http://apiservice`
- Web: `http://webservice`
- PostgreSQL: `postgres`
- Redis: `redis`

N�o h� portas fixas expostas. Para acessar externamente, utilize o dashboard Aspire ou configure endpoints p�blicos conforme necess�rio.

## Como funciona
- Aspire orquestra todos os containers na mesma rede.
- As configura��es de conex�o e URLs usam nomes de servi�o Docker.
- Comunica��o entre servi�os � autom�tica e segura na rede interna.

## Exemplo de configura��o

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
- Seguran�a
- Sem conflitos de porta

## ?? Servi�os Dispon�veis

### 1. S3-like Storage
- Buckets e objetos
- Versionamento
- Criptografia
- Controle de acesso

### 2. RDS Database
- PostgreSQL e MySQL
- Backups autom�ticos
- Conex�es gerenciadas

### 3. NoSQL Database
- Esquema flex�vel
- Escalabilidade autom�tica
- Capacidade configur�vel

### 4. Message Queue
- Filas FIFO e Standard
- Retry autom�tico
- Dead letter queues

### 5. Serverless Functions
- Runtime .NET, Python, Node.js
- Timeout configur�vel
- Mem�ria ajust�vel

## ?? Gerenciamento de Usu�rios

### Roles Dispon�veis
- **Admin**: Acesso completo ao sistema
- **Manager**: Gerenciamento de usu�rios e servi�os
- **User**: Acesso b�sico aos servi�os

### Multi-Tenancy
- Isolamento completo entre tenants
- Configura��es por tenant
- Cobran�a separada

## ?? Seguran�a

### Autentica��o
- JWT tokens seguros
- Refresh tokens
- Expira��o configur�vel

### Autoriza��o
- Role-based access control
- Pol�ticas de acesso
- API keys por servi�o

## ?? Monitoramento

### Health Checks
- Status da aplica��o
- Conectividade do banco
- Status dos servi�os

### Logging
- Logs estruturados com Serilog
- N�veis configur�veis
- Correla��o de requests

## ?? Deploy

### Desenvolvimento Local
```bash
# Com Aspire (recomendado)
dotnet run --project SSBJr.DockSaaS.AppHost

# Sem Aspire
dotnet run --project SSBJr.DockSaaS.ApiService
dotnet run --project SSBJr.DockSaaS.Web
```

### Produ��o
- Configure vari�veis de ambiente
- Use HTTPS obrigat�rio
- Configure conex�es de banco seguras
- Altere chaves JWT padr�o

## ?? Testes

```bash
# Executar todos os testes
dotnet test

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## ?? Contribui��o

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudan�as (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## ?? Licen�a

Este projeto est� licenciado sob a Licen�a MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

## ????? Autor

**Samuel da Silva B Jr**
- GitHub: [@SamuelSBJr97](https://github.com/SamuelSBJr97)
- Email: samuel@example.com

## ?? Agradecimentos

- Microsoft pela plataforma .NET e Aspire
- Comunidade MudBlazor pelos componentes UI
- Comunidade open source pelas bibliotecas utilizadas

---

? Se este projeto foi �til para voc�, considere dar uma estrela!