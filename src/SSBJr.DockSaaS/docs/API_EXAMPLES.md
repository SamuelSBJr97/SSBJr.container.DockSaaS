# DockSaaS API Examples

This document provides comprehensive examples of how to consume the DockSaaS APIs for each service type.

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
{BASE_URL}/api/{service_type}/{tenant_id}/{service_instance_id}
```

---

## ??? S3-like Storage Service

### List Buckets
```bash
curl -X GET "https://localhost:7000/api/s3storage/{tenant-id}/{service-id}/buckets" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Create Bucket
```bash
curl -X POST "https://localhost:7000/api/s3storage/{tenant-id}/{service-id}/buckets" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "my-documents",
    "versioning": true,
    "encryption": true,
    "publicAccess": false
  }'
```

### Get Bucket Details
```bash
curl -X GET "https://localhost:7000/api/s3storage/{tenant-id}/{service-id}/buckets/my-documents" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### List Objects in Bucket
```bash
curl -X GET "https://localhost:7000/api/s3storage/{tenant-id}/{service-id}/buckets/my-documents/objects?prefix=documents/&maxKeys=100" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Upload Object
```bash
curl -X POST "https://localhost:7000/api/s3storage/{tenant-id}/{service-id}/buckets/my-documents/objects" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -F "file=@document.pdf" \
  -F "key=documents/important.pdf" \
  -F "contentType=application/pdf"
```

### Download Object
```bash
curl -X GET "https://localhost:7000/api/s3storage/{tenant-id}/{service-id}/buckets/my-documents/objects/documents/important.pdf" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -o downloaded_file.pdf
```

### Delete Object
```bash
curl -X DELETE "https://localhost:7000/api/s3storage/{tenant-id}/{service-id}/buckets/my-documents/objects/documents/important.pdf" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Get Storage Usage
```bash
curl -X GET "https://localhost:7000/api/s3storage/{tenant-id}/{service-id}/usage" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

---

## ??? RDS-like Database Service

### Get Database Instance Info
```bash
curl -X GET "https://localhost:7000/api/rdsdatabase/{tenant-id}/{service-id}/info" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Execute SQL Query
```bash
curl -X POST "https://localhost:7000/api/rdsdatabase/{tenant-id}/{service-id}/query" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "SELECT * FROM users WHERE active = ? LIMIT ?",
    "parameters": [true, 10]
  }'
```

### List Tables
```bash
curl -X GET "https://localhost:7000/api/rdsdatabase/{tenant-id}/{service-id}/tables" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Create Table
```bash
curl -X POST "https://localhost:7000/api/rdsdatabase/{tenant-id}/{service-id}/tables" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "users",
    "schemaName": "public",
    "schema": {
      "id": "SERIAL PRIMARY KEY",
      "name": "VARCHAR(255) NOT NULL",
      "email": "VARCHAR(255) UNIQUE NOT NULL",
      "active": "BOOLEAN DEFAULT true",
      "created_at": "TIMESTAMP DEFAULT CURRENT_TIMESTAMP"
    }
  }'
```

### Get Table Details
```bash
curl -X GET "https://localhost:7000/api/rdsdatabase/{tenant-id}/{service-id}/tables/users" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Insert Row
```bash
curl -X POST "https://localhost:7000/api/rdsdatabase/{tenant-id}/{service-id}/tables/users/rows" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com",
    "active": true
  }'
```

### List Backups
```bash
curl -X GET "https://localhost:7000/api/rdsdatabase/{tenant-id}/{service-id}/backups" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Create Backup
```bash
curl -X POST "https://localhost:7000/api/rdsdatabase/{tenant-id}/{service-id}/backups" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "retentionDays": 30
  }'
```

### Get Database Metrics
```bash
curl -X GET "https://localhost:7000/api/rdsdatabase/{tenant-id}/{service-id}/metrics" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

---

## ?? DynamoDB-like NoSQL Service

### List Tables
```bash
curl -X GET "https://localhost:7000/api/nosqldatabase/{tenant-id}/{service-id}/tables" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Create Table
```bash
curl -X POST "https://localhost:7000/api/nosqldatabase/{tenant-id}/{service-id}/tables" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "users",
    "keySchema": [
      {
        "attributeName": "id",
        "keyType": "HASH"
      }
    ],
    "attributeDefinitions": [
      {
        "attributeName": "id",
        "attributeType": "S"
      }
    ],
    "provisionedThroughput": {
      "readCapacityUnits": 5,
      "writeCapacityUnits": 5
    }
  }'
```

### Get Table Details
```bash
curl -X GET "https://localhost:7000/api/nosqldatabase/{tenant-id}/{service-id}/tables/users" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Put Item
```bash
curl -X POST "https://localhost:7000/api/nosqldatabase/{tenant-id}/{service-id}/tables/users/items" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
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
curl -X GET "https://localhost:7000/api/nosqldatabase/{tenant-id}/{service-id}/tables/users/items/user123" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Query Items
```bash
curl -X POST "https://localhost:7000/api/nosqldatabase/{tenant-id}/{service-id}/tables/users/query" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "keyConditions": {
      "id": {
        "attributeValueList": [{"S": "user123"}],
        "comparisonOperator": "EQ"
      }
    },
    "limit": 10
  }'
```

### Scan Table
```bash
curl -X POST "https://localhost:7000/api/nosqldatabase/{tenant-id}/{service-id}/tables/users/scan" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "limit": 20,
    "filterExpression": "attribute_exists(active)"
  }'
```

### Delete Item
```bash
curl -X DELETE "https://localhost:7000/api/nosqldatabase/{tenant-id}/{service-id}/tables/users/items/user123" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

---

## ?? SQS-like Queue Service

### List Queues
```bash
curl -X GET "https://localhost:7000/api/queue/{tenant-id}/{service-id}/queues" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Create Queue
```bash
curl -X POST "https://localhost:7000/api/queue/{tenant-id}/{service-id}/queues" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "queueName": "processing-queue",
    "queueType": "Standard",
    "visibilityTimeoutSeconds": 30,
    "messageRetentionPeriod": 345600,
    "maxReceiveCount": 10,
    "delaySeconds": 0
  }'
```

### Get Queue Details
```bash
curl -X GET "https://localhost:7000/api/queue/{tenant-id}/{service-id}/queues/processing-queue" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Send Message
```bash
curl -X POST "https://localhost:7000/api/queue/{tenant-id}/{service-id}/queues/processing-queue/messages" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "messageBody": "Hello World! This is a test message.",
    "delaySeconds": 0,
    "messageAttributes": {
      "priority": {
        "stringValue": "high",
        "dataType": "String"
      },
      "source": {
        "stringValue": "api",
        "dataType": "String"
      }
    }
  }'
```

### Receive Messages
```bash
curl -X GET "https://localhost:7000/api/queue/{tenant-id}/{service-id}/queues/processing-queue/messages?maxNumberOfMessages=10&visibilityTimeoutSeconds=30&waitTimeSeconds=20" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Delete Message
```bash
curl -X DELETE "https://localhost:7000/api/queue/{tenant-id}/{service-id}/queues/processing-queue/messages/{receipt-handle}" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Send Message Batch
```bash
curl -X POST "https://localhost:7000/api/queue/{tenant-id}/{service-id}/queues/processing-queue/messages/batch" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "entries": [
      {
        "id": "msg1",
        "messageBody": "First message",
        "delaySeconds": 0
      },
      {
        "id": "msg2",
        "messageBody": "Second message",
        "delaySeconds": 5
      }
    ]
  }'
```

### Get Queue Attributes
```bash
curl -X GET "https://localhost:7000/api/queue/{tenant-id}/{service-id}/queues/processing-queue/attributes" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

---

## ? Lambda-like Functions Service

### List Functions
```bash
curl -X GET "https://localhost:7000/api/function/{tenant-id}/{service-id}/functions" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Create Function
```bash
curl -X POST "https://localhost:7000/api/function/{tenant-id}/{service-id}/functions" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "functionName": "user-processor",
    "runtime": "dotnet8",
    "role": "arn:aws:iam::123456789012:role/lambda-execution-role",
    "handler": "MyFunction.Handler",
    "code": "UEsDBAoAAAAAAKxW...", 
    "description": "Processes user data",
    "timeout": 30,
    "memorySize": 128,
    "environment": {
      "variables": {
        "LOG_LEVEL": "INFO",
        "DATABASE_URL": "postgresql://..."
      }
    }
  }'
```

### Get Function Details
```bash
curl -X GET "https://localhost:7000/api/function/{tenant-id}/{service-id}/functions/user-processor" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Update Function
```bash
curl -X PUT "https://localhost:7000/api/function/{tenant-id}/{service-id}/functions/user-processor" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Updated function description",
    "timeout": 60,
    "memorySize": 256,
    "environment": {
      "variables": {
        "LOG_LEVEL": "DEBUG"
      }
    }
  }'
```

### Invoke Function (Synchronous)
```bash
curl -X POST "https://localhost:7000/api/function/{tenant-id}/{service-id}/functions/user-processor/invoke?invocationType=RequestResponse" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "payload": {
      "userId": 123,
      "action": "process",
      "data": {
        "name": "John Doe",
        "email": "john@example.com"
      }
    }
  }'
```

### Invoke Function (Asynchronous)
```bash
curl -X POST "https://localhost:7000/api/function/{tenant-id}/{service-id}/functions/user-processor/invoke?invocationType=Event" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "payload": {
      "userId": 456,
      "action": "process_async"
    }
  }'
```

### Update Function Code
```bash
curl -X PUT "https://localhost:7000/api/function/{tenant-id}/{service-id}/functions/user-processor/code" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "zipFile": "UEsDBAoAAAAAAKxW1VIAAAAAAAAAAAUAYQAAAG15ZnVuY3Rpb24vUEsDBBQAAAAIAKxW1VI..."
  }'
```

### Get Function Configuration
```bash
curl -X GET "https://localhost:7000/api/function/{tenant-id}/{service-id}/functions/user-processor/configuration" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Get Function Invocation Metrics
```bash
curl -X GET "https://localhost:7000/api/function/{tenant-id}/{service-id}/functions/user-processor/invocations?startTime=2024-01-01T00:00:00Z&endTime=2024-01-02T00:00:00Z" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

---

## ?? Apache Kafka Service

### Get Cluster Information
```bash
curl -X GET "https://localhost:7000/api/kafka/{tenant-id}/{service-id}/cluster/info" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Get Cluster Health
```bash
curl -X GET "https://localhost:7000/api/kafka/{tenant-id}/{service-id}/cluster/health" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### List Topics
```bash
curl -X GET "https://localhost:7000/api/kafka/{tenant-id}/{service-id}/topics" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Create Topic
```bash
curl -X POST "https://localhost:7000/api/kafka/{tenant-id}/{service-id}/topics" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "user-events",
    "partitions": 6,
    "replicationFactor": 1,
    "retentionMs": 604800000,
    "compressionType": "snappy",
    "cleanupPolicy": "delete"
  }'
```

### Get Topic Details
```bash
curl -X GET "https://localhost:7000/api/kafka/{tenant-id}/{service-id}/topics/user-events" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Produce Message
```bash
curl -X POST "https://localhost:7000/api/kafka/{tenant-id}/{service-id}/topics/user-events/messages" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}" \
  -H "Content-Type: application/json" \
  -d '{
    "key": "user-123",
    "value": "{\"userId\": 123, \"action\": \"login\", \"timestamp\": \"2024-01-15T10:30:00Z\"}",
    "headers": {
      "content-type": "application/json",
      "source": "user-service"
    }
  }'
```

### Consume Messages
```bash
curl -X GET "https://localhost:7000/api/kafka/{tenant-id}/{service-id}/topics/user-events/messages?consumerGroup=analytics&maxMessages=5&timeoutMs=10000" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### List Consumer Groups
```bash
curl -X GET "https://localhost:7000/api/kafka/{tenant-id}/{service-id}/consumer-groups" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

### Delete Topic
```bash
curl -X DELETE "https://localhost:7000/api/kafka/{tenant-id}/{service-id}/topics/user-events" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-API-Key: {service-api-key}"
```

---

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

---

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

---

## SDK Examples

### JavaScript/Node.js

```javascript
const DockSaaSClient = require('@docksaas/client');

const client = new DockSaaSClient({
  baseURL: 'https://localhost:7000',
  token: 'your-jwt-token'
});

// S3 Storage - Upload file
const s3Result = await client.s3.uploadObject({
  serviceId: 'service-id',
  bucketName: 'documents',
  key: 'important.pdf',
  file: fs.createReadStream('document.pdf')
});

// RDS Database - Execute query
const rdsResult = await client.rds.executeQuery({
  serviceId: 'rds-service-id',
  query: 'SELECT * FROM users WHERE active = ?',
  parameters: [true]
});

// NoSQL Database - Put item
const nosqlResult = await client.nosql.putItem({
  serviceId: 'nosql-service-id',
  tableName: 'users',
  item: {
    id: { S: 'user123' },
    name: { S: 'John Doe' },
    email: { S: 'john@example.com' }
  }
});

// Queue - Send message
const queueResult = await client.queue.sendMessage({
  serviceId: 'queue-service-id',
  queueName: 'processing-queue',
  messageBody: 'Hello World!',
  messageAttributes: {
    priority: { stringValue: 'high', dataType: 'String' }
  }
});

// Lambda - Invoke function
const lambdaResult = await client.lambda.invoke({
  serviceId: 'lambda-service-id',
  functionName: 'user-processor',
  payload: { userId: 123, action: 'process' }
});

// Kafka - Produce message
const kafkaResult = await client.kafka.produce({
  serviceId: 'kafka-service-id',
  topic: 'user-events',
  key: 'user-123',
  value: JSON.stringify({ userId: 123, action: 'login' })
});
```

### Python

```python
from docksaas_client import DockSaaSClient

client = DockSaaSClient(
    base_url='https://localhost:7000',
    token='your-jwt-token'
)

# S3 Storage - List buckets
buckets = client.s3.list_buckets(service_id='service-id')

# RDS Database - Create table
table = client.rds.create_table(
    service_id='rds-service-id',
    table_name='users',
    schema={
        'id': 'SERIAL PRIMARY KEY',
        'name': 'VARCHAR(255) NOT NULL',
        'email': 'VARCHAR(255) UNIQUE NOT NULL'
    }
)

# NoSQL Database - Query items
items = client.nosql.query(
    service_id='nosql-service-id',
    table_name='users',
    key_conditions={
        'id': {
            'attribute_value_list': [{'S': 'user123'}],
            'comparison_operator': 'EQ'
        }
    }
)

# Queue - Receive messages
messages = client.queue.receive_messages(
    service_id='queue-service-id',
    queue_name='processing-queue',
    max_number_of_messages=10
)

# Lambda - Create function
function = client.lambda_func.create_function(
    service_id='lambda-service-id',
    function_name='data-processor',
    runtime='python3.9',
    handler='lambda_function.lambda_handler',
    code=base64.b64encode(zip_content).decode('utf-8')
)

# Kafka - Consume messages
kafka_messages = client.kafka.consume(
    service_id='kafka-service-id',
    topic='events',
    consumer_group='analytics',
    max_messages=5
)
```

### C#

```csharp
using DockSaaS.Client;

var client = new DockSaaSClient(new DockSaaSClientOptions
{
    BaseUrl = "https://localhost:7000",
    Token = "your-jwt-token"
});

// S3 Storage - Upload object
var s3Response = await client.S3.UploadObjectAsync(new UploadObjectRequest
{
    ServiceId = "service-id",
    BucketName = "documents",
    Key = "important.pdf",
    Content = fileStream,
    ContentType = "application/pdf"
});

// RDS Database - Insert row
var rdsResponse = await client.RDS.InsertRowAsync(new InsertRowRequest
{
    ServiceId = "rds-service-id",
    TableName = "users",
    Data = new Dictionary<string, object>
    {
        ["name"] = "John Doe",
        ["email"] = "john@example.com",
        ["active"] = true
    }
});

// NoSQL Database - Scan table
var nosqlResponse = await client.NoSQL.ScanAsync(new ScanRequest
{
    ServiceId = "nosql-service-id",
    TableName = "users",
    Limit = 20
});

// Queue - Send message batch
var queueResponse = await client.Queue.SendMessageBatchAsync(new SendMessageBatchRequest
{
    ServiceId = "queue-service-id",
    QueueName = "processing-queue",
    Entries = new[]
    {
        new MessageEntry { Id = "1", MessageBody = "First message" },
        new MessageEntry { Id = "2", MessageBody = "Second message" }
    }
});

// Lambda - Invoke function
var lambdaResponse = await client.Lambda.InvokeAsync(new InvokeRequest
{
    ServiceId = "lambda-service-id",
    FunctionName = "user-processor",
    Payload = JsonSerializer.Serialize(new { UserId = 123, Action = "process" })
});

// Kafka - Produce message
var kafkaResponse = await client.Kafka.ProduceAsync(new ProduceRequest
{
    ServiceId = "kafka-service-id",
    Topic = "user-events",
    Key = "user-456",
    Value = JsonSerializer.Serialize(new { UserId = 456, Action = "purchase" }),
    Headers = new Dictionary<string, string>
    {
        ["content-type"] = "application/json",
        ["source"] = "order-service"
    }
});
```

---

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

---

## Service-Specific Features

### S3 Storage Features
- Multi-part uploads for large files
- Object versioning and lifecycle policies
- Server-side encryption
- Pre-signed URLs for temporary access
- Cross-origin resource sharing (CORS)

### RDS Database Features
- Automated backups with point-in-time recovery
- Read replicas for scaling
- Performance insights and monitoring
- SSL/TLS encryption in transit
- Parameter groups for configuration

### NoSQL Database Features
- Global secondary indexes
- Local secondary indexes
- Time to live (TTL) for automatic deletion
- Point-in-time recovery
- Conditional operations

### Queue Features
- Message deduplication for FIFO queues
- Dead letter queues for failed messages
- Message attributes and filtering
- Long polling for efficient message retrieval
- Batch operations for throughput

### Lambda Features
- Environment variables and secrets
- VPC support for network isolation
- Layers for code sharing
- Versioning and aliases
- Event source mappings

### Kafka Features
- Schema Registry for data governance
- Kafka Connect for data integration
- Consumer group management
- Topic partitioning strategies
- Message compression and retention

For more detailed API documentation, visit the Swagger UI at `/swagger` when running the application.