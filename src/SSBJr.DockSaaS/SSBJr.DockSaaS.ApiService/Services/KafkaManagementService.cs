using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System.Text.Json;

namespace SSBJr.DockSaaS.ApiService.Services;

public interface IKafkaManagementService
{
    Task<bool> CreateTopicAsync(string topicName, int partitions = 3, short replicationFactor = 1);
    Task<bool> DeleteTopicAsync(string topicName);
    Task<List<string>> ListTopicsAsync();
    Task<TopicMetadata?> GetTopicInfoAsync(string topicName);
    Task<bool> ProduceMessageAsync(string topic, string key, string value, Dictionary<string, string>? headers = null);
    Task<List<ConsumeResult<string, string>>> ConsumeMessagesAsync(string topic, string consumerGroup, int maxMessages = 10, TimeSpan? timeout = null);
    Task<bool> TestConnectionAsync();
}

public class KafkaManagementService : IKafkaManagementService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaManagementService> _logger;
    private readonly string _bootstrapServers;
    private IAdminClient? _adminClient;
    private IProducer<string, string>? _producer;

    public KafkaManagementService(IConfiguration configuration, ILogger<KafkaManagementService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Get Kafka connection string from Aspire configuration
        _bootstrapServers = _configuration.GetConnectionString("kafka") ?? "localhost:9092";
        
        _logger.LogInformation("Kafka Management Service initialized with bootstrap servers: {BootstrapServers}", _bootstrapServers);
    }

    private IAdminClient GetAdminClient()
    {
        if (_adminClient == null)
        {
            var config = new AdminClientConfig
            {
                BootstrapServers = _bootstrapServers,
                SecurityProtocol = SecurityProtocol.Plaintext,
                SocketTimeoutMs = 30000
            };
            
            _adminClient = new AdminClientBuilder(config).Build();
        }
        
        return _adminClient;
    }

    private IProducer<string, string> GetProducer()
    {
        if (_producer == null)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers,
                SecurityProtocol = SecurityProtocol.Plaintext,
                Acks = Acks.All,
                RetryBackoffMs = 1000,
                MessageTimeoutMs = 30000
            };
            
            _producer = new ProducerBuilder<string, string>(config).Build();
        }
        
        return _producer;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Testing Kafka connection...");
            
            var adminClient = GetAdminClient();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            
            _logger.LogInformation("Kafka connection successful. Found {BrokerCount} brokers", metadata.Brokers.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Kafka");
            return false;
        }
    }

    public async Task<bool> CreateTopicAsync(string topicName, int partitions = 3, short replicationFactor = 1)
    {
        try
        {
            _logger.LogInformation("Creating Kafka topic: {TopicName} with {Partitions} partitions", topicName, partitions);
            
            var adminClient = GetAdminClient();
            
            var topicSpecification = new TopicSpecification
            {
                Name = topicName,
                NumPartitions = partitions,
                ReplicationFactor = replicationFactor,
                Configs = new Dictionary<string, string>
                {
                    ["cleanup.policy"] = "delete",
                    ["retention.ms"] = "604800000", // 7 days
                    ["compression.type"] = "snappy"
                }
            };

            await adminClient.CreateTopicsAsync(new[] { topicSpecification });
            _logger.LogInformation("Topic {TopicName} created successfully", topicName);
            return true;
        }
        catch (CreateTopicsException ex)
        {
            foreach (var result in ex.Results)
            {
                if (result.Error.Code == ErrorCode.TopicAlreadyExists)
                {
                    _logger.LogWarning("Topic {TopicName} already exists", result.Topic);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to create topic {TopicName}: {Error}", result.Topic, result.Error.Reason);
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating topic {TopicName}", topicName);
            return false;
        }
    }

    public async Task<bool> DeleteTopicAsync(string topicName)
    {
        try
        {
            _logger.LogInformation("Deleting Kafka topic: {TopicName}", topicName);
            
            var adminClient = GetAdminClient();
            await adminClient.DeleteTopicsAsync(new[] { topicName });
            
            _logger.LogInformation("Topic {TopicName} deleted successfully", topicName);
            return true;
        }
        catch (DeleteTopicsException ex)
        {
            foreach (var result in ex.Results)
            {
                _logger.LogError("Failed to delete topic {TopicName}: {Error}", result.Topic, result.Error.Reason);
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting topic {TopicName}", topicName);
            return false;
        }
    }

    public async Task<List<string>> ListTopicsAsync()
    {
        try
        {
            _logger.LogDebug("Listing Kafka topics");
            
            var adminClient = GetAdminClient();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            
            var topics = metadata.Topics
                .Where(t => !t.Topic.StartsWith("__")) // Filter out internal topics
                .Select(t => t.Topic)
                .OrderBy(t => t)
                .ToList();
            
            _logger.LogDebug("Found {TopicCount} topics", topics.Count);
            return await Task.FromResult(topics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Kafka topics");
            return new List<string>();
        }
    }

    public async Task<TopicMetadata?> GetTopicInfoAsync(string topicName)
    {
        try
        {
            _logger.LogDebug("Getting info for topic: {TopicName}", topicName);
            
            var adminClient = GetAdminClient();
            var metadata = adminClient.GetMetadata(topicName, TimeSpan.FromSeconds(10));
            
            var topicMetadata = metadata.Topics.FirstOrDefault(t => t.Topic == topicName);
            return await Task.FromResult(topicMetadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting topic info for {TopicName}", topicName);
            return null;
        }
    }

    public async Task<bool> ProduceMessageAsync(string topic, string key, string value, Dictionary<string, string>? headers = null)
    {
        try
        {
            _logger.LogDebug("Producing message to topic: {Topic} with key: {Key}", topic, key);
            
            var producer = GetProducer();
            
            var message = new Message<string, string>
            {
                Key = key,
                Value = value,
                Timestamp = new Timestamp(DateTime.UtcNow)
            };

            // Add headers if provided
            if (headers != null && headers.Any())
            {
                message.Headers = new Headers();
                foreach (var header in headers)
                {
                    message.Headers.Add(header.Key, System.Text.Encoding.UTF8.GetBytes(header.Value));
                }
            }

            var deliveryResult = await producer.ProduceAsync(topic, message);
            
            _logger.LogDebug("Message produced successfully to {Topic}:{Partition} at offset {Offset}", 
                deliveryResult.Topic, deliveryResult.Partition.Value, deliveryResult.Offset.Value);
            
            return true;
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to produce message to topic {Topic}: {Error}", topic, ex.Error.Reason);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error producing message to topic {Topic}", topic);
            return false;
        }
    }

    public async Task<List<ConsumeResult<string, string>>> ConsumeMessagesAsync(string topic, string consumerGroup, int maxMessages = 10, TimeSpan? timeout = null)
    {
        var messages = new List<ConsumeResult<string, string>>();
        
        try
        {
            _logger.LogDebug("Consuming messages from topic: {Topic} with consumer group: {ConsumerGroup}", topic, consumerGroup);
            
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = consumerGroup,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                SecurityProtocol = SecurityProtocol.Plaintext
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(topic);
            
            var consumeTimeout = timeout ?? TimeSpan.FromSeconds(5);
            var endTime = DateTime.UtcNow.Add(consumeTimeout);
            
            while (messages.Count < maxMessages && DateTime.UtcNow < endTime)
            {
                try
                {
                    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(1));
                    if (consumeResult != null)
                    {
                        messages.Add(consumeResult);
                        _logger.LogDebug("Consumed message from {Topic}:{Partition} at offset {Offset}", 
                            consumeResult.Topic, consumeResult.Partition.Value, consumeResult.Offset.Value);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogWarning("Consume error: {Error}", ex.Error.Reason);
                    break;
                }
            }
            
            consumer.Close();
            _logger.LogDebug("Consumed {MessageCount} messages from topic {Topic}", messages.Count, topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming messages from topic {Topic}", topic);
        }
        
        return await Task.FromResult(messages);
    }

    public void Dispose()
    {
        try
        {
            _producer?.Dispose();
            _adminClient?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing Kafka clients");
        }
    }
}