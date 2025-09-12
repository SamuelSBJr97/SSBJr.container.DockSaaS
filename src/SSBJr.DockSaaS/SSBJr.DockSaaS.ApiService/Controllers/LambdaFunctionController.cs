using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSBJr.DockSaaS.ApiService.Data;
using SSBJr.DockSaaS.ApiService.Models;
using System.Text.Json;

namespace SSBJr.DockSaaS.ApiService.Controllers;

[ApiController]
[Route("api/function/{tenantId:guid}/{serviceId:guid}")]
[Authorize]
public class LambdaFunctionController : ControllerBase
{
    private readonly DockSaaSDbContext _context;
    private readonly ILogger<LambdaFunctionController> _logger;

    public LambdaFunctionController(DockSaaSDbContext context, ILogger<LambdaFunctionController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("functions")]
    public async Task<ActionResult<FunctionListResponse>> GetFunctions()
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting function list
            var functions = new List<FunctionInfo>
            {
                new() { FunctionName = "user-processor", Runtime = "dotnet8", 
                       LastModified = DateTime.UtcNow.AddDays(-5), CodeSize = 2048576, Timeout = 30, MemorySize = 128 },
                new() { FunctionName = "data-transformer", Runtime = "python3.9", 
                       LastModified = DateTime.UtcNow.AddDays(-2), CodeSize = 1536000, Timeout = 60, MemorySize = 256 },
                new() { FunctionName = "webhook-handler", Runtime = "nodejs18", 
                       LastModified = DateTime.UtcNow.AddHours(-6), CodeSize = 3145728, Timeout = 15, MemorySize = 512 }
            };

            var response = new FunctionListResponse
            {
                Functions = functions,
                TotalFunctions = functions.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get functions for Lambda service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve functions" });
        }
    }

    [HttpPost("functions")]
    public async Task<ActionResult<FunctionInfo>> CreateFunction([FromBody] CreateFunctionRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            if (string.IsNullOrEmpty(request.FunctionName))
                return BadRequest(new { error = "Function name is required" });

            if (string.IsNullOrEmpty(request.Runtime))
                return BadRequest(new { error = "Runtime is required" });

            if (string.IsNullOrEmpty(request.Handler))
                return BadRequest(new { error = "Handler is required" });

            // Simulate function creation
            var function = new FunctionInfo
            {
                FunctionName = request.FunctionName,
                FunctionArn = $"arn:aws:lambda:us-east-1:{serviceInstance.TenantId}:function:{request.FunctionName}",
                Runtime = request.Runtime,
                Role = $"arn:aws:iam::{serviceInstance.TenantId}:role/lambda-execution-role",
                Handler = request.Handler,
                CodeSize = request.Code?.Length ?? 1024,
                Description = request.Description,
                Timeout = request.Timeout ?? 30,
                MemorySize = request.MemorySize ?? 128,
                LastModified = DateTime.UtcNow,
                State = "Active",
                StateReason = "The function is active",
                Environment = new FunctionEnvironment
                {
                    Variables = request.Environment?.Variables ?? new Dictionary<string, string>()
                }
            };

            _logger.LogInformation("Created function {FunctionName} for Lambda service {ServiceId}", 
                request.FunctionName, serviceInstance.Id);

            return CreatedAtAction(nameof(GetFunction), new { functionName = function.FunctionName }, function);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create function for Lambda service {ServiceId}", serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to create function" });
        }
    }

    [HttpGet("functions/{functionName}")]
    public async Task<ActionResult<FunctionDetails>> GetFunction(string functionName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting function details
            var random = new Random();
            var functionDetails = new FunctionDetails
            {
                FunctionName = functionName,
                FunctionArn = $"arn:aws:lambda:us-east-1:{serviceInstance.TenantId}:function:{functionName}",
                Runtime = "dotnet8",
                Role = $"arn:aws:iam::{serviceInstance.TenantId}:role/lambda-execution-role",
                Handler = "MyFunction.Handler",
                CodeSize = random.Next(1024, 10485760),
                Description = $"Function {functionName} for processing data",
                Timeout = 30,
                MemorySize = 128,
                LastModified = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                CodeSha256 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())),
                Version = "$LATEST",
                State = "Active",
                StateReason = "The function is active",
                LastUpdateStatus = "Successful",
                Environment = new FunctionEnvironment
                {
                    Variables = new Dictionary<string, string>
                    {
                        ["LOG_LEVEL"] = "INFO",
                        ["ENVIRONMENT"] = "production"
                    }
                },
                DeadLetterConfig = new FunctionDeadLetterConfig
                {
                    TargetArn = $"arn:aws:sqs:us-east-1:{serviceInstance.TenantId}:dlq"
                },
                TracingConfig = new FunctionTracingConfig
                {
                    Mode = "Active"
                }
            };

            return Ok(functionDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get function {FunctionName} for Lambda service {ServiceId}", 
                functionName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to retrieve function" });
        }
    }

    [HttpPut("functions/{functionName}")]
    public async Task<ActionResult<FunctionInfo>> UpdateFunction(string functionName, [FromBody] UpdateFunctionRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate function update
            var function = new FunctionInfo
            {
                FunctionName = functionName,
                FunctionArn = $"arn:aws:lambda:us-east-1:{serviceInstance.TenantId}:function:{functionName}",
                Runtime = request.Runtime ?? "dotnet8",
                Role = $"arn:aws:iam::{serviceInstance.TenantId}:role/lambda-execution-role",
                Handler = request.Handler ?? "MyFunction.Handler",
                CodeSize = new Random().Next(1024, 10485760),
                Description = request.Description,
                Timeout = request.Timeout ?? 30,
                MemorySize = request.MemorySize ?? 128,
                LastModified = DateTime.UtcNow,
                State = "Active",
                StateReason = "The function is active"
            };

            _logger.LogInformation("Updated function {FunctionName} for Lambda service {ServiceId}", 
                functionName, serviceInstance.Id);

            return Ok(function);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update function {FunctionName} for Lambda service {ServiceId}", 
                functionName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to update function" });
        }
    }

    [HttpDelete("functions/{functionName}")]
    public async Task<IActionResult> DeleteFunction(string functionName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate function deletion
            _logger.LogInformation("Deleted function {FunctionName} from Lambda service {ServiceId}", 
                functionName, serviceInstance.Id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete function {FunctionName} for Lambda service {ServiceId}", 
                functionName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to delete function" });
        }
    }

    [HttpPost("functions/{functionName}/invoke")]
    public async Task<ActionResult<InvokeResponse>> InvokeFunction(
        string functionName, 
        [FromBody] InvokeRequest request,
        [FromQuery] string invocationType = "RequestResponse")
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            var startTime = DateTime.UtcNow;
            
            // Simulate function execution
            await Task.Delay(new Random().Next(100, 2000)); // Simulate processing time
            
            var executionTime = DateTime.UtcNow - startTime;
            var random = new Random();

            var response = new InvokeResponse
            {
                StatusCode = 200,
                ExecutedVersion = "$LATEST",
                Payload = invocationType == "RequestResponse" ? 
                    JsonSerializer.Serialize(new { 
                        message = "Function executed successfully", 
                        input = request.Payload,
                        timestamp = DateTime.UtcNow,
                        result = $"Processed {random.Next(1, 1000)} items"
                    }) : null,
                LogResult = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                    $"START RequestId: {Guid.NewGuid()}\n" +
                    $"Function executed successfully\n" +
                    $"END RequestId: {Guid.NewGuid()}\n" +
                    $"REPORT Duration: {executionTime.TotalMilliseconds:F2} ms")),
                FunctionError = null
            };

            _logger.LogInformation("Invoked function {FunctionName} for Lambda service {ServiceId}. Execution time: {ExecutionTime}ms", 
                functionName, serviceInstance.Id, executionTime.TotalMilliseconds);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke function {FunctionName} for Lambda service {ServiceId}", 
                functionName, serviceInstance.Id);
            
            return Ok(new InvokeResponse
            {
                StatusCode = 500,
                FunctionError = "Unhandled",
                Payload = JsonSerializer.Serialize(new { 
                    errorMessage = "Internal function error",
                    errorType = "RuntimeError"
                })
            });
        }
    }

    [HttpPut("functions/{functionName}/code")]
    public async Task<ActionResult<FunctionCodeResponse>> UpdateFunctionCode(
        string functionName, 
        [FromBody] UpdateFunctionCodeRequest request)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            if (string.IsNullOrEmpty(request.ZipFile))
                return BadRequest(new { error = "ZipFile is required" });

            // Simulate code update
            var response = new FunctionCodeResponse
            {
                FunctionName = functionName,
                FunctionArn = $"arn:aws:lambda:us-east-1:{serviceInstance.TenantId}:function:{functionName}",
                CodeSize = request.ZipFile.Length / 4 * 3, // Approximate size after base64 decode
                CodeSha256 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())),
                LastModified = DateTime.UtcNow,
                Version = "$LATEST"
            };

            _logger.LogInformation("Updated code for function {FunctionName} for Lambda service {ServiceId}", 
                functionName, serviceInstance.Id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update code for function {FunctionName} for Lambda service {ServiceId}", 
                functionName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to update function code" });
        }
    }

    [HttpGet("functions/{functionName}/configuration")]
    public async Task<ActionResult<FunctionConfiguration>> GetFunctionConfiguration(string functionName)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            // Simulate getting function configuration
            var configuration = new FunctionConfiguration
            {
                FunctionName = functionName,
                FunctionArn = $"arn:aws:lambda:us-east-1:{serviceInstance.TenantId}:function:{functionName}",
                Runtime = "dotnet8",
                Role = $"arn:aws:iam::{serviceInstance.TenantId}:role/lambda-execution-role",
                Handler = "MyFunction.Handler",
                CodeSize = new Random().Next(1024, 10485760),
                Description = $"Configuration for {functionName}",
                Timeout = 30,
                MemorySize = 128,
                LastModified = DateTime.UtcNow.AddDays(-1),
                CodeSha256 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())),
                Version = "$LATEST",
                Environment = new FunctionEnvironment
                {
                    Variables = new Dictionary<string, string>
                    {
                        ["LOG_LEVEL"] = "INFO",
                        ["ENVIRONMENT"] = "production"
                    }
                }
            };

            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get configuration for function {FunctionName} for Lambda service {ServiceId}", 
                functionName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to get function configuration" });
        }
    }

    [HttpGet("functions/{functionName}/invocations")]
    public async Task<ActionResult<InvocationMetricsResponse>> GetInvocationMetrics(
        string functionName,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null)
    {
        var (serviceInstance, error) = await ValidateServiceInstanceAsync();
        if (error != null) return error;

        try
        {
            var random = new Random();
            var metrics = new InvocationMetricsResponse
            {
                FunctionName = functionName,
                Period = startTime ?? DateTime.UtcNow.AddHours(-24),
                EndTime = endTime ?? DateTime.UtcNow,
                TotalInvocations = random.Next(100, 10000),
                SuccessfulInvocations = random.Next(95, 99) * random.Next(100, 10000) / 100,
                ErrorInvocations = random.Next(1, 50),
                AverageDuration = random.NextDouble() * 1000,
                MaxDuration = random.NextDouble() * 5000,
                MinDuration = random.NextDouble() * 100,
                TotalDuration = random.NextDouble() * 100000,
                Throttles = random.Next(0, 10),
                ConcurrentExecutions = random.Next(1, 50)
            };

            metrics.SuccessRate = metrics.TotalInvocations > 0 ? 
                (double)metrics.SuccessfulInvocations / metrics.TotalInvocations * 100 : 0;

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get invocation metrics for function {FunctionName} for Lambda service {ServiceId}", 
                functionName, serviceInstance.Id);
            return StatusCode(500, new { error = "Failed to get invocation metrics" });
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

        if (serviceInstance.ServiceDefinition.Type != "Function")
            return (null!, BadRequest(new { error = "Service is not a Lambda function service" }));

        if (serviceInstance.Status != "Running")
            return (null!, BadRequest(new { error = "Lambda function service is not running" }));

        // Verify API key if provided
        var apiKey = Request.Headers["X-API-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey) && apiKey != serviceInstance.ApiKey)
            return (null!, Unauthorized(new { error = "Invalid API key" }));

        return (serviceInstance, null);
    }
}

// Lambda Function DTOs
public class FunctionListResponse
{
    public List<FunctionInfo> Functions { get; set; } = new();
    public int TotalFunctions { get; set; }
}

public class FunctionInfo
{
    public string FunctionName { get; set; } = "";
    public string? FunctionArn { get; set; }
    public string Runtime { get; set; } = "";
    public string? Role { get; set; }
    public string? Handler { get; set; }
    public long CodeSize { get; set; }
    public string? Description { get; set; }
    public int Timeout { get; set; }
    public int MemorySize { get; set; }
    public DateTime LastModified { get; set; }
    public string? CodeSha256 { get; set; }
    public string? Version { get; set; }
    public string? State { get; set; }
    public string? StateReason { get; set; }
    public string? LastUpdateStatus { get; set; }
    public FunctionEnvironment? Environment { get; set; }
}

public class CreateFunctionRequest
{
    public string FunctionName { get; set; } = "";
    public string Runtime { get; set; } = "";
    public string Role { get; set; } = "";
    public string Handler { get; set; } = "";
    public string? Code { get; set; } // Base64 encoded zip
    public string? Description { get; set; }
    public int? Timeout { get; set; }
    public int? MemorySize { get; set; }
    public FunctionEnvironment? Environment { get; set; }
}

public class UpdateFunctionRequest
{
    public string? Runtime { get; set; }
    public string? Handler { get; set; }
    public string? Description { get; set; }
    public int? Timeout { get; set; }
    public int? MemorySize { get; set; }
    public FunctionEnvironment? Environment { get; set; }
}

public class FunctionDetails : FunctionInfo
{
    public FunctionDeadLetterConfig? DeadLetterConfig { get; set; }
    public FunctionTracingConfig? TracingConfig { get; set; }
}

public class FunctionEnvironment
{
    public Dictionary<string, string> Variables { get; set; } = new();
}

public class FunctionDeadLetterConfig
{
    public string? TargetArn { get; set; }
}

public class FunctionTracingConfig
{
    public string Mode { get; set; } = "PassThrough"; // Active, PassThrough
}

public class InvokeRequest
{
    public object? Payload { get; set; }
}

public class InvokeResponse
{
    public int StatusCode { get; set; }
    public string? ExecutedVersion { get; set; }
    public string? Payload { get; set; }
    public string? LogResult { get; set; }
    public string? FunctionError { get; set; }
}

public class UpdateFunctionCodeRequest
{
    public string ZipFile { get; set; } = ""; // Base64 encoded
}

public class FunctionCodeResponse
{
    public string FunctionName { get; set; } = "";
    public string? FunctionArn { get; set; }
    public long CodeSize { get; set; }
    public string? CodeSha256 { get; set; }
    public DateTime LastModified { get; set; }
    public string? Version { get; set; }
}

public class FunctionConfiguration
{
    public string FunctionName { get; set; } = "";
    public string? FunctionArn { get; set; }
    public string Runtime { get; set; } = "";
    public string? Role { get; set; }
    public string Handler { get; set; } = "";
    public long CodeSize { get; set; }
    public string? Description { get; set; }
    public int Timeout { get; set; }
    public int MemorySize { get; set; }
    public DateTime LastModified { get; set; }
    public string? CodeSha256 { get; set; }
    public string? Version { get; set; }
    public FunctionEnvironment? Environment { get; set; }
}

public class InvocationMetricsResponse
{
    public string FunctionName { get; set; } = "";
    public DateTime Period { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalInvocations { get; set; }
    public int SuccessfulInvocations { get; set; }
    public int ErrorInvocations { get; set; }
    public double AverageDuration { get; set; }
    public double MaxDuration { get; set; }
    public double MinDuration { get; set; }
    public double TotalDuration { get; set; }
    public int Throttles { get; set; }
    public int ConcurrentExecutions { get; set; }
    public double SuccessRate { get; set; }
}