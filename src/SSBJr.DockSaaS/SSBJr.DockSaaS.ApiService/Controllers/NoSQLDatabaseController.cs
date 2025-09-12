using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSBJr.DockSaaS.ApiService.Data;
using SSBJr.DockSaaS.ApiService.Models;
using System.Text.Json;

namespace SSBJr.DockSaaS.ApiService.Controllers;

[ApiController]
[Route("api/nosqldatabase/{tenantId:guid}/{serviceId:guid}")]
[Authorize]
public class NoSQLDatabaseController : ControllerBase
{
    private readonly DockSaaSDbContext _context;
    private readonly ILogger<NoSQLDatabaseController> _logger;

    public NoSQLDatabaseController(DockSaaSDbContext context, ILogger<NoSQLDatabaseController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("tables")]
    public async Task<ActionResult<NoSQLTablesResponse>> GetTables()
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting table list
            var tables = new List<NoSQLTable>
            {
                new() { TableName = "users", Status = "ACTIVE", ItemCount = 1250, TableSizeBytes = 524288, 
                       ReadCapacityUnits = 5, WriteCapacityUnits = 5, CreationDateTime = DateTime.UtcNow.AddDays(-30) },
                new() { TableName = "orders", Status = "ACTIVE", ItemCount = 3420, TableSizeBytes = 1048576, 
                       ReadCapacityUnits = 10, WriteCapacityUnits = 10, CreationDateTime = DateTime.UtcNow.AddDays(-15) },
                new() { TableName = "products", Status = "ACTIVE", ItemCount = 856, TableSizeBytes = 262144, 
                       ReadCapacityUnits = 3, WriteCapacityUnits = 3, CreationDateTime = DateTime.UtcNow.AddDays(-7) }
            };

            var response = new NoSQLTablesResponse
            {
                Tables = tables,
                TotalTables = tables.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tables for NoSQL service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve tables" });
        }
    }

    [HttpPost("tables")]
    public async Task<ActionResult<NoSQLTable>> CreateTable([FromBody] CreateNoSQLTableRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            if (string.IsNullOrEmpty(request.TableName))
                return BadRequest(new { error = "Table name is required" });

            if (request.KeySchema == null || !request.KeySchema.Any())
                return BadRequest(new { error = "Key schema is required" });

            // Simulate table creation
            var table = new NoSQLTable
            {
                TableName = request.TableName,
                Status = "CREATING",
                ItemCount = 0,
                TableSizeBytes = 0,
                ReadCapacityUnits = request.ProvisionedThroughput?.ReadCapacityUnits ?? 5,
                WriteCapacityUnits = request.ProvisionedThroughput?.WriteCapacityUnits ?? 5,
                CreationDateTime = DateTime.UtcNow,
                KeySchema = request.KeySchema,
                AttributeDefinitions = request.AttributeDefinitions
            };

            _logger.LogInformation("Created table {TableName} for NoSQL service {ServiceId}", 
                request.TableName, serviceInstance.Id);

            return CreatedAtAction(nameof(GetTable), new { tableName = table.TableName }, table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create table for NoSQL service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to create table" });
        }
    }

    [HttpGet("tables/{tableName}")]
    public async Task<ActionResult<NoSQLTableDetails>> GetTable(string tableName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting table details
            var random = new Random();
            var tableDetails = new NoSQLTableDetails
            {
                TableName = tableName,
                Status = "ACTIVE",
                ItemCount = random.Next(100, 10000),
                TableSizeBytes = random.NextInt64(1024, 1024 * 1024 * 100),
                ReadCapacityUnits = 5,
                WriteCapacityUnits = 5,
                CreationDateTime = DateTime.UtcNow.AddDays(-random.Next(1, 365)),
                KeySchema = new List<NoSQLKeySchemaElement>
                {
                    new() { AttributeName = "id", KeyType = "HASH" }
                },
                AttributeDefinitions = new List<NoSQLAttributeDefinition>
                {
                    new() { AttributeName = "id", AttributeType = "S" }
                },
                GlobalSecondaryIndexes = new List<NoSQLGlobalSecondaryIndex>(),
                LocalSecondaryIndexes = new List<NoSQLLocalSecondaryIndex>()
            };

            return Ok(tableDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get table {TableName} for NoSQL service {ServiceId}", 
                tableName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve table" });
        }
    }

    [HttpDelete("tables/{tableName}")]
    public async Task<IActionResult> DeleteTable(string tableName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate table deletion
            _logger.LogInformation("Deleted table {TableName} from NoSQL service {ServiceId}", 
                tableName, serviceInstance.Id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete table {TableName} for NoSQL service {ServiceId}", 
                tableName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to delete table" });
        }
    }

    [HttpPost("tables/{tableName}/items")]
    public async Task<ActionResult<NoSQLPutItemResponse>> PutItem(string tableName, [FromBody] NoSQLPutItemRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            if (request.Item == null || !request.Item.Any())
                return BadRequest(new { error = "Item is required" });

            // Simulate item insertion
            var response = new NoSQLPutItemResponse
            {
                ConsumedCapacity = new NoSQLConsumedCapacity
                {
                    TableName = tableName,
                    CapacityUnits = 1.0
                },
                ItemCollectionMetrics = null
            };

            _logger.LogInformation("Put item into table {TableName} for NoSQL service {ServiceId}", 
                tableName, serviceInstance.Id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to put item into table {TableName} for NoSQL service {ServiceId}", 
                tableName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to put item" });
        }
    }

    [HttpGet("tables/{tableName}/items/{itemKey}")]
    public async Task<ActionResult<NoSQLGetItemResponse>> GetItem(string tableName, string itemKey)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate item retrieval
            var item = new Dictionary<string, NoSQLAttributeValue>
            {
                ["id"] = new() { S = itemKey },
                ["name"] = new() { S = $"Item {itemKey}" },
                ["created_at"] = new() { S = DateTime.UtcNow.ToString("O") },
                ["active"] = new() { BOOL = true },
                ["count"] = new() { N = new Random().Next(1, 100).ToString() }
            };

            var response = new NoSQLGetItemResponse
            {
                Item = item,
                ConsumedCapacity = new NoSQLConsumedCapacity
                {
                    TableName = tableName,
                    CapacityUnits = 0.5
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get item {ItemKey} from table {TableName} for NoSQL service {ServiceId}", 
                itemKey, tableName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to get item" });
        }
    }

    [HttpPost("tables/{tableName}/query")]
    public async Task<ActionResult<NoSQLQueryResponse>> Query(string tableName, [FromBody] NoSQLQueryRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate query execution
            var random = new Random();
            var itemCount = random.Next(1, 10);
            var items = new List<Dictionary<string, NoSQLAttributeValue>>();

            for (int i = 0; i < itemCount; i++)
            {
                items.Add(new Dictionary<string, NoSQLAttributeValue>
                {
                    ["id"] = new() { S = $"item-{i + 1}" },
                    ["name"] = new() { S = $"Query Result {i + 1}" },
                    ["created_at"] = new() { S = DateTime.UtcNow.AddDays(-random.Next(1, 30)).ToString("O") },
                    ["score"] = new() { N = random.Next(1, 100).ToString() }
                });
            }

            var response = new NoSQLQueryResponse
            {
                Items = items,
                Count = items.Count,
                ScannedCount = items.Count,
                ConsumedCapacity = new NoSQLConsumedCapacity
                {
                    TableName = tableName,
                    CapacityUnits = items.Count * 0.5
                }
            };

            _logger.LogInformation("Executed query on table {TableName} for NoSQL service {ServiceId}. Returned {Count} items", 
                tableName, serviceInstance.Id, items.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute query on table {TableName} for NoSQL service {ServiceId}", 
                tableName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to execute query" });
        }
    }

    [HttpPost("tables/{tableName}/scan")]
    public async Task<ActionResult<NoSQLScanResponse>> Scan(string tableName, [FromBody] NoSQLScanRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate scan execution
            var random = new Random();
            var limit = request.Limit ?? 100;
            var itemCount = Math.Min(limit, random.Next(1, 20));
            var items = new List<Dictionary<string, NoSQLAttributeValue>>();

            for (int i = 0; i < itemCount; i++)
            {
                items.Add(new Dictionary<string, NoSQLAttributeValue>
                {
                    ["id"] = new() { S = Guid.NewGuid().ToString() },
                    ["data"] = new() { S = $"Scan Result {i + 1}" },
                    ["timestamp"] = new() { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
                });
            }

            var response = new NoSQLScanResponse
            {
                Items = items,
                Count = items.Count,
                ScannedCount = random.Next(items.Count, items.Count * 3),
                ConsumedCapacity = new NoSQLConsumedCapacity
                {
                    TableName = tableName,
                    CapacityUnits = items.Count * 0.5
                }
            };

            _logger.LogInformation("Executed scan on table {TableName} for NoSQL service {ServiceId}. Returned {Count} items", 
                tableName, serviceInstance.Id, items.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute scan on table {TableName} for NoSQL service {ServiceId}", 
                tableName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to execute scan" });
        }
    }

    [HttpDelete("tables/{tableName}/items/{itemKey}")]
    public async Task<ActionResult<NoSQLDeleteItemResponse>> DeleteItem(string tableName, string itemKey)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate item deletion
            var response = new NoSQLDeleteItemResponse
            {
                ConsumedCapacity = new NoSQLConsumedCapacity
                {
                    TableName = tableName,
                    CapacityUnits = 1.0
                }
            };

            _logger.LogInformation("Deleted item {ItemKey} from table {TableName} for NoSQL service {ServiceId}", 
                itemKey, tableName, serviceInstance.Id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete item {ItemKey} from table {TableName} for NoSQL service {ServiceId}", 
                itemKey, tableName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to delete item" });
        }
    }

    private async Task<(ServiceInstance serviceInstance, ActionResult? error)> ValidateServiceInstanceAsync()
    {
        var tenantId = Guid.Parse(RouteData.Values["tenantId"]!.ToString()!);
        var serviceId = Guid.Parse(RouteData.Values["serviceId"]!.ToString()!);

        var serviceInstance = await _context.ServiceInstances
            .Include(s => s.ServiceDefinition)
            .Include(s => s.Tenant)
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.TenantId == tenantId);

        if (serviceInstance == null)
            return (null!, NotFound(new { error = "Service instance not found" }));

        if (serviceInstance.ServiceDefinition.Type != "NoSQLDatabase")
            return (null!, BadRequest(new { error = "Service is not a NoSQL database service" }));

        if (serviceInstance.Status != "Running")
            return (null!, BadRequest(new { error = "NoSQL database service is not running" }));

        // Verify API key if provided
        var apiKey = Request.Headers["X-API-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey) && apiKey != serviceInstance.ApiKey)
            return (null!, Unauthorized(new { error = "Invalid API key" }));

        return (serviceInstance, null);
    }
}

// NoSQL DTOs
public class NoSQLTablesResponse
{
    public List<NoSQLTable> Tables { get; set; } = new();
    public int TotalTables { get; set; }
}

public class NoSQLTable
{
    public string TableName { get; set; } = "";
    public string Status { get; set; } = "";
    public int ItemCount { get; set; }
    public long TableSizeBytes { get; set; }
    public int ReadCapacityUnits { get; set; }
    public int WriteCapacityUnits { get; set; }
    public DateTime CreationDateTime { get; set; }
    public List<NoSQLKeySchemaElement>? KeySchema { get; set; }
    public List<NoSQLAttributeDefinition>? AttributeDefinitions { get; set; }
}

public class CreateNoSQLTableRequest
{
    public string TableName { get; set; } = "";
    public List<NoSQLKeySchemaElement> KeySchema { get; set; } = new();
    public List<NoSQLAttributeDefinition> AttributeDefinitions { get; set; } = new();
    public NoSQLProvisionedThroughput? ProvisionedThroughput { get; set; }
}

public class NoSQLTableDetails : NoSQLTable
{
    public List<NoSQLGlobalSecondaryIndex> GlobalSecondaryIndexes { get; set; } = new();
    public List<NoSQLLocalSecondaryIndex> LocalSecondaryIndexes { get; set; } = new();
}

public class NoSQLKeySchemaElement
{
    public string AttributeName { get; set; } = "";
    public string KeyType { get; set; } = ""; // HASH or RANGE
}

public class NoSQLAttributeDefinition
{
    public string AttributeName { get; set; } = "";
    public string AttributeType { get; set; } = ""; // S, N, B
}

public class NoSQLProvisionedThroughput
{
    public int ReadCapacityUnits { get; set; }
    public int WriteCapacityUnits { get; set; }
}

public class NoSQLGlobalSecondaryIndex
{
    public string IndexName { get; set; } = "";
    public List<NoSQLKeySchemaElement> KeySchema { get; set; } = new();
    public NoSQLProvisionedThroughput ProvisionedThroughput { get; set; } = new();
}

public class NoSQLLocalSecondaryIndex
{
    public string IndexName { get; set; } = "";
    public List<NoSQLKeySchemaElement> KeySchema { get; set; } = new();
}

public class NoSQLPutItemRequest
{
    public Dictionary<string, NoSQLAttributeValue> Item { get; set; } = new();
}

public class NoSQLPutItemResponse
{
    public NoSQLConsumedCapacity? ConsumedCapacity { get; set; }
    public object? ItemCollectionMetrics { get; set; }
}

public class NoSQLGetItemResponse
{
    public Dictionary<string, NoSQLAttributeValue>? Item { get; set; }
    public NoSQLConsumedCapacity? ConsumedCapacity { get; set; }
}

public class NoSQLQueryRequest
{
    public Dictionary<string, NoSQLCondition>? KeyConditions { get; set; }
    public string? FilterExpression { get; set; }
    public int? Limit { get; set; }
    public bool? ScanIndexForward { get; set; }
}

public class NoSQLQueryResponse
{
    public List<Dictionary<string, NoSQLAttributeValue>> Items { get; set; } = new();
    public int Count { get; set; }
    public int ScannedCount { get; set; }
    public NoSQLConsumedCapacity? ConsumedCapacity { get; set; }
}

public class NoSQLScanRequest
{
    public string? FilterExpression { get; set; }
    public int? Limit { get; set; }
}

public class NoSQLScanResponse
{
    public List<Dictionary<string, NoSQLAttributeValue>> Items { get; set; } = new();
    public int Count { get; set; }
    public int ScannedCount { get; set; }
    public NoSQLConsumedCapacity? ConsumedCapacity { get; set; }
}

public class NoSQLDeleteItemResponse
{
    public NoSQLConsumedCapacity? ConsumedCapacity { get; set; }
}

public class NoSQLAttributeValue
{
    public string? S { get; set; } // String
    public string? N { get; set; } // Number
    public bool? BOOL { get; set; } // Boolean
    public List<string>? SS { get; set; } // String Set
    public List<string>? NS { get; set; } // Number Set
}

public class NoSQLCondition
{
    public List<NoSQLAttributeValue> AttributeValueList { get; set; } = new();
    public string ComparisonOperator { get; set; } = "";
}

public class NoSQLConsumedCapacity
{
    public string TableName { get; set; } = "";
    public double CapacityUnits { get; set; }
}