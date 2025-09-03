# DockSaaS - AWS-like Services Platform

DockSaaS is a comprehensive SaaS platform that allows administrators to create and manage AWS-like services that can be dynamically instantiated and consumed via REST APIs. Built with .NET 8, Blazor Server, PostgreSQL, and Docker.

## ?? Features

### Core Platform Features
- **Multi-tenant Architecture**: Isolated tenant environments with quota management
- **JWT Authentication**: Secure authentication with role-based access control (RBAC)
- **OAuth2/SSO Support**: Integration with Google, Microsoft, and LDAP
- **Responsive Blazor UI**: Modern, responsive web interface built with MudBlazor

### AWS-like Services
The platform supports dynamic creation and management of the following service types:

1. **S3-like Storage**: Object storage with configurable quotas and encryption
2. **RDS-like Database**: Relational database instances with multi-engine support
3. **DynamoDB-like NoSQL**: NoSQL database with configurable billing modes
4. **SQS-like Queue**: Message queuing with configurable retention and visibility
5. **Lambda-like Functions**: Serverless functions with runtime selection
6. **CloudWatch-like Monitoring**: Metrics collection and real-time monitoring
7. **IAM-like Control**: Granular access control and API key management
8. **SNS-like Notifications**: Email, push, and webhook notifications

### Management Features
- **Real-time Dashboard**: Service metrics, usage statistics, and tenant overview
- **Service Lifecycle Management**: Start, stop, configure, and monitor services
- **Audit Logging**: Complete activity tracking and compliance reporting
- **Usage Monitoring**: Resource consumption tracking with quota enforcement
- **API Management**: Automatic REST/GraphQL API generation for external consumption

## ??? Architecture

```
???????????????????    ???????????????????    ???????????????????
?   Blazor Web    ??????   API Service   ??????   PostgreSQL    ?
?   (Frontend)    ?    ?   (Backend)     ?    ?   (Database)    ?
???????????????????    ???????????????????    ???????????????????
                              ?
                       ???????????????????
                       ?      Redis      ?
                       ?   (Caching)     ?
                       ???????????????????
```

## ??? Technology Stack

- **Frontend**: Blazor Server, MudBlazor UI Components
- **Backend**: ASP.NET Core 8, Minimal APIs
- **Database**: PostgreSQL with Entity Framework Core
- **Caching**: Redis (via .NET Aspire)
- **Authentication**: JWT, ASP.NET Core Identity
- **Containerization**: Docker & Docker Compose
- **Orchestration**: .NET Aspire for development

## ?? Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [PostgreSQL](https://www.postgresql.org/download/) (for local development)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## ?? Quick Start

### Option 1: Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/SamuelSBJr97/SSBjr.container.DockSaaS.git
   cd SSBjr.container.DockSaaS
   ```

2. **Start all services**
   ```bash
   docker-compose up -d
   ```

3. **Access the application**
   - Web Interface: http://localhost:5001
   - API Documentation: http://localhost:5000/swagger

### Option 2: Local Development with .NET Aspire

1. **Clone and restore packages**
   ```bash
   git clone https://github.com/SamuelSBJr97/SSBjr.container.DockSaaS.git
   cd SSBjr.container.DockSaaS/src/SSBjr.container.DockSaaS
   dotnet restore
   ```

2. **Start PostgreSQL and Redis**
   ```bash
   docker run -d --name postgres -p 5432:5432 -e POSTGRES_DB=docksaas -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres postgres:15
   docker run -d --name redis -p 6379:6379 redis:7-alpine
   ```

3. **Run the application**
   ```bash
   dotnet run --project SSBJr.container.DockSaaS.AppHost
   ```

4. **Access via Aspire Dashboard**
   - Aspire Dashboard: https://localhost:15888
   - Web Interface: https://localhost:7001
   - API Service: https://localhost:7000

## ?? Usage Guide

### Getting Started

1. **Create Account**: Navigate to the web interface and register a new account
2. **Tenant Setup**: Your first registration automatically creates a tenant and assigns you admin role
3. **Dashboard**: Access the dashboard to view tenant statistics and service overview

### Creating Services

1. **Navigate to Services**: Use the sidebar menu to access "Services > Create Service"
2. **Select Service Type**: Choose from available AWS-like services (S3, RDS, DynamoDB, etc.)
3. **Configure Service**: Set name, quotas, and service-specific configuration
4. **Deploy**: Click "Create Service" to instantiate the service

### Managing Services

- **Start/Stop**: Control service lifecycle from the services list or dashboard
- **Monitor**: View real-time metrics and usage statistics
- **Configure**: Modify service settings and quotas
- **API Access**: Each service automatically gets a REST endpoint and API key

### API Consumption

Each service instance generates endpoints for external consumption:

```bash
# Example: S3-like Storage Service
curl -X POST "https://api.docksaas.local/s3storage/{tenant-id}/{service-id}/upload" \
  -H "Authorization: Bearer {api-key}" \
  -F "file=@document.pdf"

# Example: Queue Service
curl -X POST "https://api.docksaas.local/queue/{tenant-id}/{service-id}/send" \
  -H "Authorization: Bearer {api-key}" \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello World"}'
```

## ?? Configuration

### Environment Variables

#### API Service
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=localhost;Database=docksaas;Username=postgres;Password=postgres
JwtSettings__Secret=YourSecretKey
JwtSettings__Issuer=DockSaaS
JwtSettings__Audience=DockSaaS-Users
```

#### Web Application
```bash
ASPNETCORE_ENVIRONMENT=Development
ApiSettings__BaseUrl=https://localhost:7000
```

### Database Migration

The application automatically runs migrations on startup. For manual migration:

```bash
cd SSBJr.container.DockSaaS.ApiService
dotnet ef database update
```

## ?? Testing

### Running Tests
```bash
dotnet test SSBJr.container.DockSaaS.Tests
```

### API Testing
Use the included Swagger UI at `/swagger` endpoint for interactive API testing.

### Service Testing Examples

```bash
# Test S3-like Storage
curl -X GET "http://localhost:5000/api/services/instances" \
  -H "Authorization: Bearer {your-jwt-token}"

# Test Service Creation
curl -X POST "http://localhost:5000/api/services/instances" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "my-storage",
    "serviceDefinitionId": "11111111-1111-1111-1111-111111111111",
    "configuration": "{\"bucketName\":\"my-bucket\",\"encryption\":true}",
    "usageQuota": 1073741824
  }'
```

## ?? Deployment

### Production Docker Deployment

1. **Build production images**
   ```bash
   docker-compose -f docker-compose.prod.yml build
   ```

2. **Deploy with production configuration**
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```

### Kubernetes Deployment
Kubernetes manifests are available in the `/k8s` directory:

```bash
kubectl apply -f k8s/
```

### Environment-specific Configurations
- Development: `appsettings.Development.json`
- Staging: `appsettings.Staging.json`
- Production: `appsettings.Production.json`

## ?? Monitoring & Observability

### Built-in Monitoring
- **Service Metrics**: CPU, memory, storage usage per service
- **Tenant Analytics**: Usage statistics and quota tracking
- **Audit Logs**: Complete activity tracking with filtering
- **Real-time Dashboard**: Live metrics and service status

### External Monitoring Integration
The platform supports integration with:
- Prometheus/Grafana for metrics
- ELK Stack for log aggregation
- Application Insights for application monitoring

## ?? Security

### Authentication & Authorization
- JWT-based authentication with configurable expiration
- Role-based access control (Admin, Manager, User)
- OAuth2 integration for enterprise SSO
- API key management for service access

### Data Security
- Multi-tenant data isolation
- Encrypted sensitive configuration
- Audit logging for compliance
- Rate limiting and quota enforcement

## ?? Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines
- Follow C# coding conventions
- Write unit tests for new features
- Update documentation for API changes
- Ensure Docker builds pass

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ?? Support

- **Documentation**: [Wiki](https://github.com/SamuelSBJr97/SSBjr.container.DockSaaS/wiki)
- **Issues**: [GitHub Issues](https://github.com/SamuelSBJr97/SSBjr.container.DockSaaS/issues)
- **Discussions**: [GitHub Discussions](https://github.com/SamuelSBJr97/SSBjr.container.DockSaaS/discussions)
- **Email**: support@docksaas.com

## ??? Roadmap

### Phase 1 (Current)
- ? Core platform infrastructure
- ? Basic AWS-like services
- ? Multi-tenant support
- ? Blazor web interface

### Phase 2 (Next)
- [ ] Advanced service configurations
- [ ] Service templates and marketplace
- [ ] Enhanced monitoring and alerting
- [ ] GraphQL API support
- [ ] Mobile responsive improvements

### Phase 3 (Future)
- [ ] Multi-cloud provider support
- [ ] Advanced analytics and reporting
- [ ] Service mesh integration
- [ ] Kubernetes operator
- [ ] Plugin system for custom services

## ?? Architecture Decisions

### Why Blazor Server?
- Real-time updates with SignalR
- Rich client-side interactions
- Server-side rendering for SEO
- Shared code between client and server

### Why PostgreSQL?
- Advanced JSON support for configuration
- Excellent performance for multi-tenant scenarios
- Strong ACID compliance
- Rich extension ecosystem

### Why .NET Aspire?
- Simplified local development experience
- Built-in observability and service discovery
- Container orchestration for development
- Production-ready deployment patterns

---

**DockSaaS** - Empowering developers with cloud-native service management platform.

Made with ?? by [Samuel da Silva B Jr](https://github.com/SamuelSBJr97)