using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSBJr.DockSaaS.ApiService.Services;
using SSBJr.DockSaaS.ApiService.DTOs;
using System.Security.Claims;
using SSBJr.DockSaaS.ApiService.Models;

namespace SSBJr.DockSaaS.ApiService.Controllers;

[ApiController]
[Route("api/kafka")]
[Authorize]
public class KafkaController : ControllerBase
{
    private readonly IKafkaManagementService _kafkaService;
    private readonly IServiceManager _serviceManager;
    private readonly ILogger<KafkaController> _logger;

    public KafkaController(
        IKafkaManagementService kafkaService,
        IServiceManager serviceManager,
        ILogger<KafkaController> logger)
    {
        _kafkaService = kafkaService;
        _serviceManager = serviceManager;
        _logger = logger;
    }

    /// <summary>
    /// Get Kafka cluster information
    /// </summary>
    [HttpGet("{tenantId}/{serviceId}/cluster/info")]
    public async Task<ActionResult<object>> GetClusterInfo(Guid tenantId, Guid serviceId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            // Validate service access
            var serviceInstance = await _serviceManager.GetServiceInstanceAsync(serviceId, userId);
            if (serviceInstance == null || serviceInstance.TenantId != tenantId)
            {
                return NotFound("Service instance not found or access denied.");
            }

            // Test connection
            var isConnected = await _kafkaService.TestConnectionAsync();
            if (!isConnected)
            {
                return StatusCode(503, new { error = "Kafka cluster is not available" });
            }

            // Get topics list for cluster info
            var topics = await _kafkaService.ListTopicsAsync();

            var clusterInfo = new
            {
                status = "healthy",
                connected = true,
                topicCount = topics.Count,
                serviceId = serviceId,
                tenantId = tenantId,
                configuration = serviceInstance.Configuration
            };

            _logger.LogInformation("Cluster info retrieved for service {ServiceId}", serviceId);
            return Ok(clusterInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cluster info for service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// List all topics in the Kafka cluster
    /// </summary>
    [HttpGet("{tenantId}/{serviceId}/topics")]
    public async Task<ActionResult<List<object>>> ListTopics(Guid tenantId, Guid serviceId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            // Validate service access
            var serviceInstance = await _serviceManager.GetServiceInstanceAsync(serviceId, userId);
            if (serviceInstance == null || serviceInstance.TenantId != tenantId)
            {
                return NotFound("Service instance not found or access denied.");
            }

            var topics = await _kafkaService.ListTopicsAsync();
            
            var topicDetails = new List<object>();
            foreach (var topicName in topics)
            {
                var topicInfo = await _kafkaService.GetTopicInfoAsync(topicName);
                topicDetails.Add(new
                {
                    name = topicName,
                    partitions = topicInfo?.Partitions?.Count ?? 0,
                    replicationFactor = topicInfo?.Partitions?.FirstOrDefault()?.Replicas?.Length ?? 0
                });
            }

            _logger.LogInformation("Listed {TopicCount} topics for service {ServiceId}", topics.Count, serviceId);
            return Ok(topicDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing topics for service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new Kafka topic
    /// </summary>
    [HttpPost("{tenantId}/{serviceId}/topics")]
    public async Task<ActionResult<object>> CreateTopic(Guid tenantId, Guid serviceId, [FromBody] CreateTopicRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            // Validate service access
            var serviceInstance = await _serviceManager.GetServiceInstanceAsync(serviceId, userId);
            if (serviceInstance == null || serviceInstance.TenantId != tenantId)
            {
                return NotFound("Service instance not found or access denied.");
            }

            // Validate request
            if (string.IsNullOrEmpty(request.Name))
            {
                return BadRequest("Topic name is required.");
            }

            if (request.Partitions <= 0)
            {
                request.Partitions = 3; // Default
            }

            if (request.ReplicationFactor <= 0)
            {
                request.ReplicationFactor = 1; // Default for single-node setup
            }

            var success = await _kafkaService.CreateTopicAsync(request.Name, request.Partitions, (short)request.ReplicationFactor);
            
            if (success)
            {
                var response = new
                {
                    name = request.Name,
                    partitions = request.Partitions,
                    replicationFactor = request.ReplicationFactor,
                    created = true,
                    createdAt = DateTime.UtcNow
                };

                _logger.LogInformation("Topic {TopicName} created successfully for service {ServiceId}", request.Name, serviceId);
                return CreatedAtAction(nameof(GetTopicInfo), new { tenantId, serviceId, topicName = request.Name }, response);
            }
            else
            {
                return BadRequest(new { error = "Failed to create topic" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating topic {TopicName} for service {ServiceId}", request.Name, serviceId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get information about a specific topic
    /// </summary>
    [HttpGet("{tenantId}/{serviceId}/topics/{topicName}")]
    public async Task<ActionResult<object>> GetTopicInfo(Guid tenantId, Guid serviceId, string topicName)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            // Validate service access
            var serviceInstance = await _serviceManager.GetServiceInstanceAsync(serviceId, userId);
            if (serviceInstance == null || serviceInstance.TenantId != tenantId)
            {
                return NotFound("Service instance not found or access denied.");
            }

            var topicInfo = await _kafkaService.GetTopicInfoAsync(topicName);
            if (topicInfo == null)
            {
                return NotFound($"Topic '{topicName}' not found.");
            }

            var response = new
            {
                name = topicInfo.Topic,
                partitions = topicInfo.Partitions.Select(p => new
                {
                    partition = p.PartitionId,
                    leader = p.Leader,
                    replicas = p.Replicas?.Length ?? 0,
                    isr = p.InSyncReplicas?.Length ?? 0
                }).ToList(),
                partitionCount = topicInfo.Partitions.Count,
                replicationFactor = topicInfo.Partitions.FirstOrDefault()?.Replicas?.Length ?? 0
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting topic info for {TopicName} in service {ServiceId}", topicName, serviceId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a topic
    /// </summary>
    [HttpDelete("{tenantId}/{serviceId}/topics/{topicName}")]
    public async Task<ActionResult> DeleteTopic(Guid tenantId, Guid serviceId, string topicName)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            // Validate service access
            var serviceInstance = await _serviceManager.GetServiceInstanceAsync(serviceId, userId);
            if (serviceInstance == null || serviceInstance.TenantId != tenantId)
            {
                return NotFound("Service instance not found or access denied.");
            }

            var success = await _kafkaService.DeleteTopicAsync(topicName);
            
            if (success)
            {
                _logger.LogInformation("Topic {TopicName} deleted successfully for service {ServiceId}", topicName, serviceId);
                return NoContent();
            }
            else
            {
                return BadRequest(new { error = "Failed to delete topic" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting topic {TopicName} for service {ServiceId}", topicName, serviceId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Produce a message to a topic
    /// </summary>
    [HttpPost("{tenantId}/{serviceId}/topics/{topicName}/messages")]
    public async Task<ActionResult<object>> ProduceMessage(Guid tenantId, Guid serviceId, string topicName, [FromBody] ProduceMessageRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            // Validate service access
            var serviceInstance = await _serviceManager.GetServiceInstanceAsync(serviceId, userId);
            if (serviceInstance == null || serviceInstance.TenantId != tenantId)
            {
                return NotFound("Service instance not found or access denied.");
            }

            // Validate request
            if (string.IsNullOrEmpty(request.Value))
            {
                return BadRequest("Message value is required.");
            }

            var success = await _kafkaService.ProduceMessageAsync(topicName, request.Key, request.Value, request.Headers);
            
            if (success)
            {
                var response = new
                {
                    topic = topicName,
                    key = request.Key,
                    value = request.Value,
                    headers = request.Headers,
                    produced = true,
                    timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Message produced to topic {TopicName} for service {ServiceId}", topicName, serviceId);
                return Ok(response);
            }
            else
            {
                return BadRequest(new { error = "Failed to produce message" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing message to topic {TopicName} for service {ServiceId}", topicName, serviceId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Consume messages from a topic
    /// </summary>
    [HttpGet("{tenantId}/{serviceId}/topics/{topicName}/messages")]
    public async Task<ActionResult<List<object>>> ConsumeMessages(Guid tenantId, Guid serviceId, string topicName, [FromQuery] string consumerGroup = "docksaas-consumer", [FromQuery] int maxMessages = 10, [FromQuery] int timeoutSeconds = 5)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            // Validate service access
            var serviceInstance = await _serviceManager.GetServiceInstanceAsync(serviceId, userId);
            if (serviceInstance == null || serviceInstance.TenantId != tenantId)
            {
                return NotFound("Service instance not found or access denied.");
            }

            var timeout = TimeSpan.FromSeconds(Math.Max(1, Math.Min(30, timeoutSeconds))); // Limit between 1-30 seconds
            var messages = await _kafkaService.ConsumeMessagesAsync(topicName, consumerGroup, maxMessages, timeout);
            
            var response = messages.Select(m => new
            {
                topic = m.Topic,
                partition = m.Partition.Value,
                offset = m.Offset.Value,
                key = m.Message.Key,
                value = m.Message.Value,
                timestamp = m.Message.Timestamp.UtcDateTime,
                headers = m.Message.Headers?.ToDictionary(h => h.Key, h => System.Text.Encoding.UTF8.GetString(h.GetValueBytes()))
            }).ToList();

            _logger.LogInformation("Consumed {MessageCount} messages from topic {TopicName} for service {ServiceId}", messages.Count, topicName, serviceId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming messages from topic {TopicName} for service {ServiceId}", topicName, serviceId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Test Kafka connection
    /// </summary>
    [HttpGet("{tenantId}/{serviceId}/health")]
    public async Task<ActionResult<object>> HealthCheck(Guid tenantId, Guid serviceId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            // Validate service access
            var serviceInstance = await _serviceManager.GetServiceInstanceAsync(serviceId, userId);
            if (serviceInstance == null || serviceInstance.TenantId != tenantId)
            {
                return NotFound("Service instance not found or access denied.");
            }

            var isHealthy = await _kafkaService.TestConnectionAsync();
            
            var response = new
            {
                status = isHealthy ? "healthy" : "unhealthy",
                connected = isHealthy,
                timestamp = DateTime.UtcNow,
                serviceId = serviceId
            };

            if (isHealthy)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(503, response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Kafka health for service {ServiceId}", serviceId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}

// DTOs for Kafka operations
public class CreateTopicRequest
{
    public string Name { get; set; } = "";
    public int Partitions { get; set; } = 3;
    public int ReplicationFactor { get; set; } = 1;
}

public class ProduceMessageRequest
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public Dictionary<string, string>? Headers { get; set; }
}