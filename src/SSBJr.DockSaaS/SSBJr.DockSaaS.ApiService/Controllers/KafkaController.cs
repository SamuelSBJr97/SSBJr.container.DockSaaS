using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSBJr.DockSaaS.ApiService.Data;
using SSBJr.DockSaaS.ApiService.Models;
using System.Text.Json;

namespace SSBJr.DockSaaS.ApiService.Controllers;

[ApiController]
[Route("api/kafka/{tenantId:guid}/{serviceId:guid}")]
[Authorize]
public class KafkaController : ControllerBase
{
    private readonly DockSaaSDbContext _context;
    private readonly ILogger<KafkaController> _logger;

    public KafkaController(DockSaaSDbContext context, ILogger<KafkaController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("topics")]
    public async Task<ActionResult<KafkaTopicsResponse>> GetTopics()
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting topics from Kafka cluster
            var topics = new List<KafkaTopic>
            {
                new() { Name = "events", Partitions = 3, ReplicationFactor = 1, RetentionMs = 604800000 },
                new() { Name = "logs", Partitions = 5, ReplicationFactor = 1, RetentionMs = 259200000 },
                new() { Name = "notifications", Partitions = 2, ReplicationFactor = 1, RetentionMs = 86400000 },
                new() { Name = "metrics", Partitions = 8, ReplicationFactor = 1, RetentionMs = 604800000 }
            };

            var response = new KafkaTopicsResponse
            {
                Topics = topics,
                TotalTopics = topics.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get topics for Kafka service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve topics" });
        }
    }

    [HttpPost("topics")]
    public async Task<ActionResult<KafkaTopic>> CreateTopic([FromBody] CreateTopicRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Validate topic name
            if (string.IsNullOrEmpty(request.Name) || request.Name.Length > 249)
                return BadRequest(new { error = "Topic name must be between 1 and 249 characters" });

            // Simulate creating topic in Kafka cluster
            var topic = new KafkaTopic
            {
                Name = request.Name,
                Partitions = request.Partitions ?? 3,
                ReplicationFactor = request.ReplicationFactor ?? 1,
                RetentionMs = request.RetentionMs ?? 604800000, // 7 days default
                CompressionType = request.CompressionType ?? "snappy",
                CleanupPolicy = request.CleanupPolicy ?? "delete"
            };

            _logger.LogInformation("Created topic {TopicName} for Kafka service {ServiceId}", 
                topic.Name, serviceInstance.Id);

            return CreatedAtAction(nameof(GetTopic), new { topicName = topic.Name }, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create topic for Kafka service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to create topic" });
        }
    }

    [HttpGet("topics/{topicName}")]
    public async Task<ActionResult<KafkaTopic>> GetTopic(string topicName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting topic details from Kafka cluster
            var topic = new KafkaTopic
            {
                Name = topicName,
                Partitions = 3,
                ReplicationFactor = 1,
                RetentionMs = 604800000,
                CompressionType = "snappy",
                CleanupPolicy = "delete",
                TotalMessages = new Random().NextInt64(10000, 1000000),
                TotalSizeBytes = new Random().NextInt64(1024 * 1024, 1024L * 1024 * 1024 * 5)
            };

            return Ok(topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get topic {TopicName} for Kafka service {ServiceId}", 
                topicName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve topic" });
        }
    }

    [HttpDelete("topics/{topicName}")]
    public async Task<IActionResult> DeleteTopic(string topicName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate deleting topic from Kafka cluster
            _logger.LogInformation("Deleted topic {TopicName} from Kafka service {ServiceId}", 
                topicName, serviceInstance.Id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete topic {TopicName} for Kafka service {ServiceId}", 
                topicName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to delete topic" });
        }
    }

    [HttpPost("topics/{topicName}/messages")]
    public async Task<ActionResult<ProduceMessageResponse>> ProduceMessage(string topicName, [FromBody] ProduceMessageRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate producing message to Kafka topic
            var messageId = Guid.NewGuid().ToString();
            var offset = new Random().NextInt64(1000000);
            var partition = new Random().Next(0, 3); // Assuming 3 partitions

            var response = new ProduceMessageResponse
            {
                MessageId = messageId,
                Topic = topicName,
                Partition = partition,
                Offset = offset,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogDebug("Produced message {MessageId} to topic {TopicName} for Kafka service {ServiceId}", 
                messageId, topicName, serviceInstance.Id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to produce message to topic {TopicName} for Kafka service {ServiceId}", 
                topicName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to produce message" });
        }
    }

    [HttpGet("topics/{topicName}/messages")]
    public async Task<ActionResult<ConsumeMessagesResponse>> ConsumeMessages(
        string topicName, 
        [FromQuery] string? consumerGroup = null,
        [FromQuery] int maxMessages = 10,
        [FromQuery] int timeoutMs = 5000)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate consuming messages from Kafka topic
            var messages = new List<KafkaMessage>();
            var messageCount = new Random().Next(0, Math.Min(maxMessages, 20));

            for (int i = 0; i < messageCount; i++)
            {
                messages.Add(new KafkaMessage
                {
                    Key = $"key-{i}",
                    Value = $"{{\"id\": {i}, \"message\": \"Sample message {i}\", \"timestamp\": \"{DateTime.UtcNow:O}\"}}",
                    Topic = topicName,
                    Partition = new Random().Next(0, 3),
                    Offset = new Random().NextInt64(1000000) + i,
                    Timestamp = DateTime.UtcNow.AddSeconds(-i * 10),
                    Headers = new Dictionary<string, string>
                    {
                        ["content-type"] = "application/json",
                        ["source"] = "kafka-api"
                    }
                });
            }

            var response = new ConsumeMessagesResponse
            {
                Messages = messages,
                ConsumerGroup = consumerGroup ?? "default-consumer-group",
                Topic = topicName,
                MessageCount = messages.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to consume messages from topic {TopicName} for Kafka service {ServiceId}", 
                topicName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to consume messages" });
        }
    }

    [HttpGet("consumer-groups")]
    public async Task<ActionResult<ConsumerGroupsResponse>> GetConsumerGroups()
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting consumer groups from Kafka cluster
            var consumerGroups = new List<ConsumerGroup>
            {
                new() 
                { 
                    GroupId = "analytics-group", 
                    State = "Stable", 
                    Members = 3,
                    Lag = new Random().Next(0, 1000)
                },
                new() 
                { 
                    GroupId = "logging-group", 
                    State = "Stable", 
                    Members = 2,
                    Lag = new Random().Next(0, 500)
                },
                new() 
                { 
                    GroupId = "monitoring-group", 
                    State = "Rebalancing", 
                    Members = 1,
                    Lag = new Random().Next(0, 2000)
                }
            };

            var response = new ConsumerGroupsResponse
            {
                ConsumerGroups = consumerGroups,
                TotalGroups = consumerGroups.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get consumer groups for Kafka service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve consumer groups" });
        }
    }

    [HttpGet("cluster/info")]
    public async Task<ActionResult<KafkaClusterInfo>> GetClusterInfo()
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            var clusterInfo = new KafkaClusterInfo
            {
                ClusterId = $"kafka-cluster-{serviceInstance.TenantId}",
                Brokers = new List<KafkaBroker>
                {
                    new() { Id = 0, Host = "broker-0", Port = 9092, Rack = "rack-a" },
                    new() { Id = 1, Host = "broker-1", Port = 9092, Rack = "rack-b" },
                    new() { Id = 2, Host = "broker-2", Port = 9092, Rack = "rack-c" }
                },
                Controller = 0,
                Version = "3.6.0"
            };

            return Ok(clusterInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cluster info for Kafka service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve cluster info" });
        }
    }

    [HttpGet("cluster/health")]
    public async Task<ActionResult<KafkaClusterHealth>> GetClusterHealth()
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            var random = new Random();
            var health = new KafkaClusterHealth
            {
                ClusterId = $"kafka-cluster-{serviceInstance.TenantId}",
                Status = "Healthy",
                BrokerCount = 3,
                OnlineBrokers = 3,
                TopicCount = random.Next(5, 50),
                PartitionCount = random.Next(15, 150),
                ConsumerGroupCount = random.Next(2, 20),
                UnderReplicatedPartitions = random.Next(0, 2),
                OfflinePartitions = random.Next(0, 1),
                LastChecked = DateTime.UtcNow,
                Metrics = new Dictionary<string, double>
                {
                    ["message_rate"] = random.Next(100, 10000),
                    ["byte_rate"] = random.Next(1024, 1024 * 1024 * 10),
                    ["network_request_rate"] = random.Next(50, 5000),
                    ["cpu_utilization"] = random.NextDouble() * 100,
                    ["memory_utilization"] = random.NextDouble() * 100,
                    ["disk_utilization"] = random.NextDouble() * 80
                }
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cluster health for Kafka service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve cluster health" });
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

        if (serviceInstance.ServiceDefinition.Type != "Kafka")
            return (null!, BadRequest(new { error = "Service is not a Kafka cluster" }));

        if (serviceInstance.Status != "Running")
            return (null!, BadRequest(new { error = "Kafka cluster is not running" }));

        // Verify API key if provided
        var apiKey = Request.Headers["X-API-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey) && apiKey != serviceInstance.ApiKey)
            return (null!, Unauthorized(new { error = "Invalid API key" }));

        return (serviceInstance, null);
    }
}

// Kafka DTOs
public class KafkaTopicsResponse
{
    public List<KafkaTopic> Topics { get; set; } = new();
    public int TotalTopics { get; set; }
}

public class KafkaTopic
{
    public string Name { get; set; } = "";
    public int Partitions { get; set; }
    public int ReplicationFactor { get; set; }
    public long RetentionMs { get; set; }
    public string CompressionType { get; set; } = "";
    public string CleanupPolicy { get; set; } = "";
    public long? TotalMessages { get; set; }
    public long? TotalSizeBytes { get; set; }
}

public class CreateTopicRequest
{
    public string Name { get; set; } = "";
    public int? Partitions { get; set; }
    public int? ReplicationFactor { get; set; }
    public long? RetentionMs { get; set; }
    public string? CompressionType { get; set; }
    public string? CleanupPolicy { get; set; }
}

public class ProduceMessageRequest
{
    public string? Key { get; set; }
    public string Value { get; set; } = "";
    public Dictionary<string, string>? Headers { get; set; }
    public int? Partition { get; set; }
}

public class ProduceMessageResponse
{
    public string MessageId { get; set; } = "";
    public string Topic { get; set; } = "";
    public int Partition { get; set; }
    public long Offset { get; set; }
    public DateTime Timestamp { get; set; }
}

public class KafkaMessage
{
    public string? Key { get; set; }
    public string Value { get; set; } = "";
    public string Topic { get; set; } = "";
    public int Partition { get; set; }
    public long Offset { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}

public class ConsumeMessagesResponse
{
    public List<KafkaMessage> Messages { get; set; } = new();
    public string ConsumerGroup { get; set; } = "";
    public string Topic { get; set; } = "";
    public int MessageCount { get; set; }
}

public class ConsumerGroupsResponse
{
    public List<ConsumerGroup> ConsumerGroups { get; set; } = new();
    public int TotalGroups { get; set; }
}

public class ConsumerGroup
{
    public string GroupId { get; set; } = "";
    public string State { get; set; } = "";
    public int Members { get; set; }
    public long Lag { get; set; }
}

public class KafkaClusterInfo
{
    public string ClusterId { get; set; } = "";
    public List<KafkaBroker> Brokers { get; set; } = new();
    public int Controller { get; set; }
    public string Version { get; set; } = "";
}

public class KafkaBroker
{
    public int Id { get; set; }
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string? Rack { get; set; }
}

public class KafkaClusterHealth
{
    public string ClusterId { get; set; } = "";
    public string Status { get; set; } = "";
    public int BrokerCount { get; set; }
    public int OnlineBrokers { get; set; }
    public int TopicCount { get; set; }
    public int PartitionCount { get; set; }
    public int ConsumerGroupCount { get; set; }
    public int UnderReplicatedPartitions { get; set; }
    public int OfflinePartitions { get; set; }
    public DateTime LastChecked { get; set; }
    public Dictionary<string, double> Metrics { get; set; } = new();
}