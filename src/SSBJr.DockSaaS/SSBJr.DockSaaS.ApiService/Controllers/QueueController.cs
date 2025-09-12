using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSBJr.DockSaaS.ApiService.Data;
using SSBJr.DockSaaS.ApiService.Models;
using System.Text.Json;

namespace SSBJr.DockSaaS.ApiService.Controllers;

[ApiController]
[Route("api/queue/{tenantId:guid}/{serviceId:guid}")]
[Authorize]
public class QueueController : ControllerBase
{
    private readonly DockSaaSDbContext _context;
    private readonly ILogger<QueueController> _logger;

    public QueueController(DockSaaSDbContext context, ILogger<QueueController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("queues")]
    public async Task<ActionResult<QueueListResponse>> GetQueues()
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting queue list
            var queues = new List<QueueInfo>
            {
                new() { QueueName = "main-queue", QueueUrl = $"https://sqs.aws.com/{serviceInstance.TenantId}/main-queue", 
                       VisibleMessages = 15, InFlightMessages = 3, DelayedMessages = 0 },
                new() { QueueName = "dlq-queue", QueueUrl = $"https://sqs.aws.com/{serviceInstance.TenantId}/dlq-queue", 
                       VisibleMessages = 2, InFlightMessages = 0, DelayedMessages = 0 },
                new() { QueueName = "processing-queue", QueueUrl = $"https://sqs.aws.com/{serviceInstance.TenantId}/processing-queue", 
                       VisibleMessages = 42, InFlightMessages = 8, DelayedMessages = 1 }
            };

            var response = new QueueListResponse
            {
                Queues = queues,
                TotalQueues = queues.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queues for Queue service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve queues" });
        }
    }

    [HttpPost("queues")]
    public async Task<ActionResult<QueueInfo>> CreateQueue([FromBody] CreateQueueRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            if (string.IsNullOrEmpty(request.QueueName))
                return BadRequest(new { error = "Queue name is required" });

            // Simulate queue creation
            var queue = new QueueInfo
            {
                QueueName = request.QueueName,
                QueueUrl = $"https://sqs.aws.com/{serviceInstance.TenantId}/{request.QueueName}",
                VisibleMessages = 0,
                InFlightMessages = 0,
                DelayedMessages = 0,
                CreatedTimestamp = DateTime.UtcNow,
                Attributes = new Dictionary<string, string>
                {
                    ["QueueArn"] = $"arn:aws:sqs:us-east-1:{serviceInstance.TenantId}:{request.QueueName}",
                    ["VisibilityTimeoutSeconds"] = (request.VisibilityTimeoutSeconds ?? 30).ToString(),
                    ["MessageRetentionPeriod"] = (request.MessageRetentionPeriod ?? 345600).ToString(),
                    ["MaxReceiveCount"] = (request.MaxReceiveCount ?? 10).ToString(),
                    ["DelaySeconds"] = (request.DelaySeconds ?? 0).ToString(),
                    ["QueueType"] = request.QueueType ?? "Standard"
                }
            };

            _logger.LogInformation("Created queue {QueueName} for Queue service {ServiceId}", 
                request.QueueName, serviceInstance.Id);

            return CreatedAtAction(nameof(GetQueue), new { queueName = queue.QueueName }, queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create queue for Queue service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to create queue" });
        }
    }

    [HttpGet("queues/{queueName}")]
    public async Task<ActionResult<QueueDetails>> GetQueue(string queueName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting queue details
            var random = new Random();
            var queueDetails = new QueueDetails
            {
                QueueName = queueName,
                QueueUrl = $"https://sqs.aws.com/{serviceInstance.TenantId}/{queueName}",
                VisibleMessages = random.Next(0, 100),
                InFlightMessages = random.Next(0, 20),
                DelayedMessages = random.Next(0, 5),
                CreatedTimestamp = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                LastModifiedTimestamp = DateTime.UtcNow.AddHours(-random.Next(1, 24)),
                Attributes = new Dictionary<string, string>
                {
                    ["QueueArn"] = $"arn:aws:sqs:us-east-1:{serviceInstance.TenantId}:{queueName}",
                    ["VisibilityTimeoutSeconds"] = "30",
                    ["MessageRetentionPeriod"] = "345600",
                    ["MaxReceiveCount"] = "10",
                    ["DelaySeconds"] = "0",
                    ["QueueType"] = "Standard",
                    ["ReceiveMessageWaitTimeSeconds"] = "0"
                }
            };

            return Ok(queueDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue {QueueName} for Queue service {ServiceId}", 
                queueName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve queue" });
        }
    }

    [HttpDelete("queues/{queueName}")]
    public async Task<IActionResult> DeleteQueue(string queueName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate queue deletion
            _logger.LogInformation("Deleted queue {QueueName} from Queue service {ServiceId}", 
                queueName, serviceInstance.Id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete queue {QueueName} for Queue service {ServiceId}", 
                queueName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to delete queue" });
        }
    }

    [HttpPost("queues/{queueName}/messages")]
    public async Task<ActionResult<SendMessageResponse>> SendMessage(string queueName, [FromBody] SendMessageRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            if (string.IsNullOrEmpty(request.MessageBody))
                return BadRequest(new { error = "Message body is required" });

            // Simulate message sending
            var response = new SendMessageResponse
            {
                MessageId = Guid.NewGuid().ToString(),
                MD5OfBody = CalculateMD5(request.MessageBody),
                MD5OfMessageAttributes = request.MessageAttributes != null ? CalculateMD5(JsonSerializer.Serialize(request.MessageAttributes)) : null,
                SequenceNumber = request.MessageGroupId != null ? new Random().Next(1000000, 9999999).ToString() : null
            };

            _logger.LogInformation("Sent message {MessageId} to queue {QueueName} for Queue service {ServiceId}", 
                response.MessageId, queueName, serviceInstance.Id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to queue {QueueName} for Queue service {ServiceId}", 
                queueName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to send message" });
        }
    }

    [HttpGet("queues/{queueName}/messages")]
    public async Task<ActionResult<ReceiveMessageResponse>> ReceiveMessages(
        string queueName,
        [FromQuery] int maxNumberOfMessages = 1,
        [FromQuery] int visibilityTimeoutSeconds = 30,
        [FromQuery] int waitTimeSeconds = 0)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate message receiving
            var random = new Random();
            var messageCount = Math.Min(maxNumberOfMessages, random.Next(0, maxNumberOfMessages + 1));
            var messages = new List<QueueMessage>();

            for (int i = 0; i < messageCount; i++)
            {
                var messageBody = $"{{\"id\": {i + 1}, \"message\": \"Sample message {i + 1}\", \"timestamp\": \"{DateTime.UtcNow:O}\"}}";
                messages.Add(new QueueMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    ReceiptHandle = Guid.NewGuid().ToString("N"),
                    MD5OfBody = CalculateMD5(messageBody),
                    Body = messageBody,
                    Attributes = new Dictionary<string, string>
                    {
                        ["SentTimestamp"] = DateTimeOffset.UtcNow.AddMinutes(-random.Next(1, 60)).ToUnixTimeMilliseconds().ToString(),
                        ["ApproximateReceiveCount"] = random.Next(1, 3).ToString(),
                        ["ApproximateFirstReceiveTimestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
                    },
                    MessageAttributes = new Dictionary<string, QueueMessageAttribute>
                    {
                        ["priority"] = new() { StringValue = "normal", DataType = "String" },
                        ["source"] = new() { StringValue = "api", DataType = "String" }
                    }
                });
            }

            var response = new ReceiveMessageResponse
            {
                Messages = messages
            };

            _logger.LogInformation("Received {Count} messages from queue {QueueName} for Queue service {ServiceId}", 
                messages.Count, queueName, serviceInstance.Id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to receive messages from queue {QueueName} for Queue service {ServiceId}", 
                queueName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to receive messages" });
        }
    }

    [HttpDelete("queues/{queueName}/messages/{receiptHandle}")]
    public async Task<IActionResult> DeleteMessage(string queueName, string receiptHandle)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate message deletion
            _logger.LogInformation("Deleted message with receipt handle {ReceiptHandle} from queue {QueueName} for Queue service {ServiceId}", 
                receiptHandle, queueName, serviceInstance.Id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete message from queue {QueueName} for Queue service {ServiceId}", 
                queueName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to delete message" });
        }
    }

    [HttpPost("queues/{queueName}/messages/batch")]
    public async Task<ActionResult<SendMessageBatchResponse>> SendMessageBatch(string queueName, [FromBody] SendMessageBatchRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            if (request.Entries == null || !request.Entries.Any())
                return BadRequest(new { error = "Message entries are required" });

            if (request.Entries.Count > 10)
                return BadRequest(new { error = "Maximum 10 messages allowed per batch" });

            // Simulate batch message sending
            var successful = new List<SendMessageBatchResultEntry>();
            var failed = new List<BatchResultErrorEntry>();

            foreach (var entry in request.Entries)
            {
                if (string.IsNullOrEmpty(entry.MessageBody))
                {
                    failed.Add(new BatchResultErrorEntry
                    {
                        Id = entry.Id,
                        Code = "EmptyBody",
                        Message = "Message body cannot be empty",
                        SenderFault = true
                    });
                    continue;
                }

                successful.Add(new SendMessageBatchResultEntry
                {
                    Id = entry.Id,
                    MessageId = Guid.NewGuid().ToString(),
                    MD5OfBody = CalculateMD5(entry.MessageBody)
                });
            }

            var response = new SendMessageBatchResponse
            {
                Successful = successful,
                Failed = failed
            };

            _logger.LogInformation("Sent batch of {SuccessCount} messages to queue {QueueName} for Queue service {ServiceId}", 
                successful.Count, queueName, serviceInstance.Id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message batch to queue {QueueName} for Queue service {ServiceId}", 
                queueName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to send message batch" });
        }
    }

    [HttpGet("queues/{queueName}/attributes")]
    public async Task<ActionResult<Dictionary<string, string>>> GetQueueAttributes(string queueName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting queue attributes
            var attributes = new Dictionary<string, string>
            {
                ["QueueArn"] = $"arn:aws:sqs:us-east-1:{serviceInstance.TenantId}:{queueName}",
                ["ApproximateNumberOfMessages"] = new Random().Next(0, 100).ToString(),
                ["ApproximateNumberOfMessagesNotVisible"] = new Random().Next(0, 20).ToString(),
                ["ApproximateNumberOfMessagesDelayed"] = new Random().Next(0, 5).ToString(),
                ["CreatedTimestamp"] = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds().ToString(),
                ["LastModifiedTimestamp"] = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds().ToString(),
                ["VisibilityTimeoutSeconds"] = "30",
                ["MessageRetentionPeriod"] = "345600",
                ["DelaySeconds"] = "0",
                ["ReceiveMessageWaitTimeSeconds"] = "0",
                ["MaxReceiveCount"] = "10"
            };

            return Ok(attributes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get attributes for queue {QueueName} for Queue service {ServiceId}", 
                queueName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to get queue attributes" });
        }
    }

    private string CalculateMD5(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
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

        if (serviceInstance.ServiceDefinition.Type != "Queue")
            return (null!, BadRequest(new { error = "Service is not a queue service" }));

        if (serviceInstance.Status != "Running")
            return (null!, BadRequest(new { error = "Queue service is not running" }));

        // Verify API key if provided
        var apiKey = Request.Headers["X-API-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey) && apiKey != serviceInstance.ApiKey)
            return (null!, Unauthorized(new { error = "Invalid API key" }));

        return (serviceInstance, null);
    }
}

// Queue DTOs
public class QueueListResponse
{
    public List<QueueInfo> Queues { get; set; } = new();
    public int TotalQueues { get; set; }
}

public class QueueInfo
{
    public string QueueName { get; set; } = "";
    public string QueueUrl { get; set; } = "";
    public int VisibleMessages { get; set; }
    public int InFlightMessages { get; set; }
    public int DelayedMessages { get; set; }
    public DateTime? CreatedTimestamp { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}

public class CreateQueueRequest
{
    public string QueueName { get; set; } = "";
    public string? QueueType { get; set; } // Standard or FIFO
    public int? VisibilityTimeoutSeconds { get; set; }
    public int? MessageRetentionPeriod { get; set; }
    public int? MaxReceiveCount { get; set; }
    public int? DelaySeconds { get; set; }
}

public class QueueDetails : QueueInfo
{
    public DateTime? LastModifiedTimestamp { get; set; }
}

public class SendMessageRequest
{
    public string MessageBody { get; set; } = "";
    public int? DelaySeconds { get; set; }
    public string? MessageGroupId { get; set; } // For FIFO queues
    public string? MessageDeduplicationId { get; set; } // For FIFO queues
    public Dictionary<string, QueueMessageAttribute>? MessageAttributes { get; set; }
}

public class SendMessageResponse
{
    public string MessageId { get; set; } = "";
    public string MD5OfBody { get; set; } = "";
    public string? MD5OfMessageAttributes { get; set; }
    public string? SequenceNumber { get; set; } // For FIFO queues
}

public class ReceiveMessageResponse
{
    public List<QueueMessage> Messages { get; set; } = new();
}

public class QueueMessage
{
    public string MessageId { get; set; } = "";
    public string ReceiptHandle { get; set; } = "";
    public string MD5OfBody { get; set; } = "";
    public string Body { get; set; } = "";
    public Dictionary<string, string>? Attributes { get; set; }
    public Dictionary<string, QueueMessageAttribute>? MessageAttributes { get; set; }
}

public class QueueMessageAttribute
{
    public string? StringValue { get; set; }
    public string? BinaryValue { get; set; }
    public string DataType { get; set; } = "String";
}

public class SendMessageBatchRequest
{
    public List<SendMessageBatchRequestEntry> Entries { get; set; } = new();
}

public class SendMessageBatchRequestEntry
{
    public string Id { get; set; } = "";
    public string MessageBody { get; set; } = "";
    public int? DelaySeconds { get; set; }
    public string? MessageGroupId { get; set; }
    public string? MessageDeduplicationId { get; set; }
    public Dictionary<string, QueueMessageAttribute>? MessageAttributes { get; set; }
}

public class SendMessageBatchResponse
{
    public List<SendMessageBatchResultEntry> Successful { get; set; } = new();
    public List<BatchResultErrorEntry> Failed { get; set; } = new();
}

public class SendMessageBatchResultEntry
{
    public string Id { get; set; } = "";
    public string MessageId { get; set; } = "";
    public string MD5OfBody { get; set; } = "";
    public string? MD5OfMessageAttributes { get; set; }
    public string? SequenceNumber { get; set; }
}

public class BatchResultErrorEntry
{
    public string Id { get; set; } = "";
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public bool SenderFault { get; set; }
}