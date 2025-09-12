using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSBJr.DockSaaS.ApiService.Data;
using SSBJr.DockSaaS.ApiService.Models;
using System.Text.Json;

namespace SSBJr.DockSaaS.ApiService.Controllers;

[ApiController]
[Route("api/s3storage/{tenantId:guid}/{serviceId:guid}")]
[Authorize]
public class S3StorageController : ControllerBase
{
    private readonly DockSaaSDbContext _context;
    private readonly ILogger<S3StorageController> _logger;

    public S3StorageController(DockSaaSDbContext context, ILogger<S3StorageController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("buckets")]
    public async Task<ActionResult<S3BucketsResponse>> GetBuckets()
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting buckets from S3 storage
            var buckets = new List<S3Bucket>
            {
                new() { Name = "documents", CreationDate = DateTime.UtcNow.AddDays(-30), Objects = 156, SizeBytes = 52428800 },
                new() { Name = "images", CreationDate = DateTime.UtcNow.AddDays(-15), Objects = 89, SizeBytes = 104857600 },
                new() { Name = "backups", CreationDate = DateTime.UtcNow.AddDays(-7), Objects = 24, SizeBytes = 2147483648 }
            };

            var response = new S3BucketsResponse
            {
                Buckets = buckets,
                TotalBuckets = buckets.Count,
                TotalObjects = buckets.Sum(b => b.Objects),
                TotalSizeBytes = buckets.Sum(b => b.SizeBytes)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get buckets for S3 service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve buckets" });
        }
    }

    [HttpPost("buckets")]
    public async Task<ActionResult<S3Bucket>> CreateBucket([FromBody] CreateBucketRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Validate bucket name
            if (string.IsNullOrEmpty(request.Name) || request.Name.Length < 3 || request.Name.Length > 63)
                return BadRequest(new { error = "Bucket name must be between 3 and 63 characters" });

            // Simulate creating bucket
            var bucket = new S3Bucket
            {
                Name = request.Name,
                CreationDate = DateTime.UtcNow,
                Objects = 0,
                SizeBytes = 0,
                Versioning = request.Versioning ?? false,
                Encryption = request.Encryption ?? true,
                PublicAccess = request.PublicAccess ?? false
            };

            _logger.LogInformation("Created bucket {BucketName} for S3 service {ServiceId}", 
                bucket.Name, serviceInstance.Id);

            return CreatedAtAction(nameof(GetBucket), new { bucketName = bucket.Name }, bucket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create bucket for S3 service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to create bucket" });
        }
    }

    [HttpGet("buckets/{bucketName}")]
    public async Task<ActionResult<S3Bucket>> GetBucket(string bucketName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting bucket details
            var bucket = new S3Bucket
            {
                Name = bucketName,
                CreationDate = DateTime.UtcNow.AddDays(-10),
                Objects = new Random().Next(10, 500),
                SizeBytes = new Random().NextInt64(1024 * 1024, 1024L * 1024 * 1024 * 5),
                Versioning = true,
                Encryption = true,
                PublicAccess = false
            };

            return Ok(bucket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get bucket {BucketName} for S3 service {ServiceId}", 
                bucketName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve bucket" });
        }
    }

    [HttpDelete("buckets/{bucketName}")]
    public async Task<IActionResult> DeleteBucket(string bucketName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate deleting bucket
            _logger.LogInformation("Deleted bucket {BucketName} from S3 service {ServiceId}", 
                bucketName, serviceInstance.Id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete bucket {BucketName} for S3 service {ServiceId}", 
                bucketName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to delete bucket" });
        }
    }

    [HttpGet("buckets/{bucketName}/objects")]
    public async Task<ActionResult<S3ObjectsResponse>> GetObjects(
        string bucketName, 
        [FromQuery] string? prefix = null,
        [FromQuery] int maxKeys = 1000)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting objects from bucket
            var objects = new List<S3Object>();
            var objectCount = new Random().Next(5, Math.Min(maxKeys, 50));

            for (int i = 0; i < objectCount; i++)
            {
                var key = prefix != null ? $"{prefix}/object-{i}.txt" : $"folder-{i % 3}/object-{i}.txt";
                objects.Add(new S3Object
                {
                    Key = key,
                    LastModified = DateTime.UtcNow.AddDays(-new Random().Next(1, 30)),
                    Size = new Random().Next(1024, 1024 * 1024),
                    ETag = Guid.NewGuid().ToString("N")[..16],
                    StorageClass = "STANDARD"
                });
            }

            var response = new S3ObjectsResponse
            {
                Objects = objects,
                BucketName = bucketName,
                Prefix = prefix,
                TotalObjects = objects.Count,
                IsTruncated = false
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get objects from bucket {BucketName} for S3 service {ServiceId}", 
                bucketName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve objects" });
        }
    }

    [HttpPost("buckets/{bucketName}/objects")]
    public async Task<ActionResult<S3UploadResponse>> UploadObject(
        string bucketName, 
        [FromForm] IFormFile file,
        [FromForm] string key,
        [FromForm] string? contentType = null)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "File is required" });

            if (string.IsNullOrEmpty(key))
                return BadRequest(new { error = "Object key is required" });

            // Simulate file upload
            var response = new S3UploadResponse
            {
                Key = key,
                BucketName = bucketName,
                Size = file.Length,
                ETag = Guid.NewGuid().ToString("N")[..16],
                ContentType = contentType ?? file.ContentType,
                UploadId = Guid.NewGuid().ToString(),
                Location = $"https://s3.amazonaws.com/{bucketName}/{key}"
            };

            _logger.LogInformation("Uploaded object {Key} to bucket {BucketName} for S3 service {ServiceId}", 
                key, bucketName, serviceInstance.Id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload object to bucket {BucketName} for S3 service {ServiceId}", 
                bucketName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to upload object" });
        }
    }

    [HttpGet("buckets/{bucketName}/objects/{*objectKey}")]
    public async Task<ActionResult> DownloadObject(string bucketName, string objectKey)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate file download
            var content = System.Text.Encoding.UTF8.GetBytes($"Simulated content for {objectKey}");
            
            _logger.LogInformation("Downloaded object {Key} from bucket {BucketName} for S3 service {ServiceId}", 
                objectKey, bucketName, serviceInstance.Id);

            return File(content, "application/octet-stream", Path.GetFileName(objectKey));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download object {Key} from bucket {BucketName} for S3 service {ServiceId}", 
                objectKey, bucketName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to download object" });
        }
    }

    [HttpDelete("buckets/{bucketName}/objects/{*objectKey}")]
    public async Task<IActionResult> DeleteObject(string bucketName, string objectKey)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate object deletion
            _logger.LogInformation("Deleted object {Key} from bucket {BucketName} for S3 service {ServiceId}", 
                objectKey, bucketName, serviceInstance.Id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete object {Key} from bucket {BucketName} for S3 service {ServiceId}", 
                objectKey, bucketName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to delete object" });
        }
    }

    [HttpGet("usage")]
    public async Task<ActionResult<S3UsageResponse>> GetUsage()
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            var random = new Random();
            var usage = new S3UsageResponse
            {
                TotalBuckets = random.Next(1, 10),
                TotalObjects = random.Next(100, 10000),
                TotalSizeBytes = random.NextInt64(1024 * 1024, 1024L * 1024 * 1024 * 100),
                RequestsLastMonth = random.Next(1000, 100000),
                DataTransferBytes = random.NextInt64(1024 * 1024, 1024L * 1024 * 1024 * 10),
                StorageCostUSD = Math.Round(random.NextDouble() * 100, 2)
            };

            return Ok(usage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get usage for S3 service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve usage" });
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

        if (serviceInstance.ServiceDefinition.Type != "S3Storage")
            return (null!, BadRequest(new { error = "Service is not an S3 storage service" }));

        if (serviceInstance.Status != "Running")
            return (null!, BadRequest(new { error = "S3 storage service is not running" }));

        // Verify API key if provided
        var apiKey = Request.Headers["X-API-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey) && apiKey != serviceInstance.ApiKey)
            return (null!, Unauthorized(new { error = "Invalid API key" }));

        return (serviceInstance, null);
    }
}

// S3 DTOs
public class S3BucketsResponse
{
    public List<S3Bucket> Buckets { get; set; } = new();
    public int TotalBuckets { get; set; }
    public int TotalObjects { get; set; }
    public long TotalSizeBytes { get; set; }
}

public class S3Bucket
{
    public string Name { get; set; } = "";
    public DateTime CreationDate { get; set; }
    public int Objects { get; set; }
    public long SizeBytes { get; set; }
    public bool Versioning { get; set; }
    public bool Encryption { get; set; }
    public bool PublicAccess { get; set; }
}

public class CreateBucketRequest
{
    public string Name { get; set; } = "";
    public bool? Versioning { get; set; }
    public bool? Encryption { get; set; }
    public bool? PublicAccess { get; set; }
}

public class S3ObjectsResponse
{
    public List<S3Object> Objects { get; set; } = new();
    public string BucketName { get; set; } = "";
    public string? Prefix { get; set; }
    public int TotalObjects { get; set; }
    public bool IsTruncated { get; set; }
}

public class S3Object
{
    public string Key { get; set; } = "";
    public DateTime LastModified { get; set; }
    public long Size { get; set; }
    public string ETag { get; set; } = "";
    public string StorageClass { get; set; } = "";
}

public class S3UploadResponse
{
    public string Key { get; set; } = "";
    public string BucketName { get; set; } = "";
    public long Size { get; set; }
    public string ETag { get; set; } = "";
    public string ContentType { get; set; } = "";
    public string UploadId { get; set; } = "";
    public string Location { get; set; } = "";
}

public class S3UsageResponse
{
    public int TotalBuckets { get; set; }
    public int TotalObjects { get; set; }
    public long TotalSizeBytes { get; set; }
    public int RequestsLastMonth { get; set; }
    public long DataTransferBytes { get; set; }
    public double StorageCostUSD { get; set; }
}