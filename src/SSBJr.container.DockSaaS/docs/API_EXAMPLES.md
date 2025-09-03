# DockSaaS API Examples

This document provides examples of how to consume the DockSaaS APIs for each service type.

## Authentication

All API calls require a valid JWT token in the Authorization header:

```bash
Authorization: Bearer YOUR_JWT_TOKEN
```

## Base URLs

- Development: `https://localhost:7000`
- Production: `https://api.docksaas.com`

## Service Endpoints

Each service instance gets its own endpoint in the format:
```
{BASE_URL}/{service_type}/{tenant_id}/{service_instance_id}
```

## S3-like Storage Service

### Upload File
```bash
curl -X POST "https://localhost:7000/s3storage/{tenant-id}/{service-id}/upload" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -F "file=@document.pdf" \
  -F "key=documents/important.pdf"
```

### Download File
```bash
curl -X GET "https://localhost:7000/s3storage/{tenant-id}/{service-id}/download/documents/important.pdf" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -o downloaded_file.pdf
```

### List Objects
```bash
curl -X GET "https://localhost:7000/s3storage/{tenant-id}/{service-id}/objects" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Delete Object
```bash
curl -X DELETE "https://localhost:7000/s3storage/{tenant-id}/{service-id}/objects/documents/important.pdf" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

## RDS-like Database Service

### Execute Query
```bash
curl -X POST "https://localhost:7000/rdsdatabase/{tenant-id}/{service-id}/query" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "SELECT * FROM users WHERE active = ?",
    "parameters": [true]
  }'
```

### Create Table
```bash
curl -X POST "https://localhost:7000/rdsdatabase/{tenant-id}/{service-id}/tables" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "users",
    "schema": {
      "id": "int primary key auto_increment",
      "name": "varchar(255) not null",
      "email": "varchar(255) unique not null",
      "active": "boolean default true",
      "created_at": "timestamp default current_timestamp"
    }
  }'
```

### Insert Data
```bash
curl -X POST "https://localhost:7000/rdsdatabase/{tenant-id}/{service-id}/tables/users/rows" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com",
    "active": true
  }'
```

## DynamoDB-like NoSQL Service

### Put Item
```bash
curl -X POST "https://localhost:7000/nosqldatabase/{tenant-id}/{service-id}/items" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "users",
    "item": {
      "id": {"S": "user123"},
      "name": {"S": "John Doe"},
      "email": {"S": "john@example.com"},
      "age": {"N": "30"},
      "active": {"BOOL": true}
    }
  }'
```

### Get Item
```bash
curl -X GET "https://localhost:7000/nosqldatabase/{tenant-id}/{service-id}/items/users/user123" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Query Items
```bash
curl -X POST "https://localhost:7000/nosqldatabase/{tenant-id}/{service-id}/query" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "users",
    "keyConditions": {
      "id": {
        "attributeValueList": [{"S": "user123"}],
        "comparisonOperator": "EQ"
      }
    }
  }'
```

## SQS-like Queue Service

### Send Message
```bash
curl -X POST "https://localhost:7000/queue/{tenant-id}/{service-id}/messages" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "messageBody": "Hello World!",
    "attributes": {
      "priority": "high",
      "source": "api"
    },
    "delaySeconds": 0
  }'
```

### Receive Messages
```bash
curl -X GET "https://localhost:7000/queue/{tenant-id}/{service-id}/messages?maxMessages=10&waitTimeSeconds=20" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Delete Message
```bash
curl -X DELETE "https://localhost:7000/queue/{tenant-id}/{service-id}/messages/{receipt-handle}" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Get Queue Attributes
```bash
curl -X GET "https://localhost:7000/queue/{tenant-id}/{service-id}/attributes" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

## Lambda-like Functions Service

### Invoke Function
```bash
curl -X POST "https://localhost:7000/function/{tenant-id}/{service-id}/invoke" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "payload": {
      "name": "John",
      "operation": "greet"
    }
  }'
```

### Update Function Code
```bash
curl -X PUT "https://localhost:7000/function/{tenant-id}/{service-id}/code" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "runtime": "dotnet8",
    "handler": "MyFunction.Handler",
    "code": "base64-encoded-zip-file"
  }'
```

### Get Function Configuration
```bash
curl -X GET "https://localhost:7000/function/{tenant-id}/{service-id}/configuration" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

## CloudWatch-like Monitoring Service

### Put Metric Data
```bash
curl -X POST "https://localhost:7000/monitoring/{tenant-id}/{service-id}/metrics" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "namespace": "MyApp/Performance",
    "metricData": [
      {
        "metricName": "ResponseTime",
        "value": 150.5,
        "unit": "Milliseconds",
        "timestamp": "2024-01-15T10:30:00Z",
        "dimensions": [
          {
            "name": "Environment",
            "value": "Production"
          }
        ]
      }
    ]
  }'
```

### Get Metric Statistics
```bash
curl -X GET "https://localhost:7000/monitoring/{tenant-id}/{service-id}/metrics/statistics?metricName=ResponseTime&startTime=2024-01-15T00:00:00Z&endTime=2024-01-15T23:59:59Z&period=300&statistics=Average,Maximum" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

## Error Handling

All endpoints return errors in the following format:

```json
{
  "error": {
    "code": "InvalidRequest",
    "message": "The request is invalid",
    "details": "Missing required parameter: file"
  },
  "requestId": "550e8400-e29b-41d4-a716-446655440000"
}
```

Common HTTP status codes:
- `200` - Success
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `429` - Too Many Requests (Rate Limited)
- `500` - Internal Server Error
- `503` - Service Unavailable

## Rate Limiting

All APIs are subject to rate limiting based on your tenant's plan:

- **Free Plan**: 1,000 requests per hour
- **Pro Plan**: 10,000 requests per hour  
- **Enterprise Plan**: 100,000 requests per hour

Rate limit headers are included in responses:

```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1642771200
```

## SDK Examples

### JavaScript/Node.js

```javascript
const DockSaaSClient = require('@docksaas/client');

const client = new DockSaaSClient({
  baseURL: 'https://localhost:7000',
  token: 'your-jwt-token'
});

// Upload file to S3-like storage
const result = await client.storage.upload({
  serviceId: 'service-id',
  file: fs.createReadStream('document.pdf'),
  key: 'documents/important.pdf'
});

console.log('Upload result:', result);
```

### Python

```python
from docksaas_client import DockSaaSClient

client = DockSaaSClient(
    base_url='https://localhost:7000',
    token='your-jwt-token'
)

# Send message to queue
result = client.queue.send_message(
    service_id='service-id',
    message_body='Hello World!',
    attributes={'priority': 'high'}
)

print(f"Message sent: {result}")
```

### C#

```csharp
using DockSaaS.Client;

var client = new DockSaaSClient(new DockSaaSClientOptions
{
    BaseUrl = "https://localhost:7000",
    Token = "your-jwt-token"
});

// Query NoSQL database
var result = await client.NoSQL.QueryAsync(new QueryRequest
{
    ServiceId = "service-id",
    TableName = "users",
    KeyConditions = new Dictionary<string, Condition>
    {
        ["id"] = new Condition
        {
            AttributeValueList = new[] { new AttributeValue { S = "user123" } },
            ComparisonOperator = "EQ"
        }
    }
});

Console.WriteLine($"Query result: {result}");
```

## Webhooks

DockSaaS can send webhooks for various events. Configure webhook URLs in your tenant settings.

### Webhook Payload Example

```json
{
  "eventType": "service.created",
  "timestamp": "2024-01-15T10:30:00Z",
  "tenantId": "tenant-123",
  "serviceId": "service-456",
  "data": {
    "serviceName": "my-storage",
    "serviceType": "S3Storage",
    "status": "running"
  }
}
```

### Verifying Webhook Signatures

```javascript
const crypto = require('crypto');

function verifyWebhookSignature(payload, signature, secret) {
  const expectedSignature = crypto
    .createHmac('sha256', secret)
    .update(payload)
    .digest('hex');
  
  return signature === `sha256=${expectedSignature}`;
}
```

For more detailed API documentation, visit the Swagger UI at `/swagger` when running the application.