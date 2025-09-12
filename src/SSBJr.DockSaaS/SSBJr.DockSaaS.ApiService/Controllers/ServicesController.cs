using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSBJr.DockSaaS.ApiService.Data;
using SSBJr.DockSaaS.ApiService.DTOs;
using SSBJr.DockSaaS.ApiService.Models;
using SSBJr.DockSaaS.ApiService.Services;
using System.Security.Claims;

namespace SSBJr.DockSaaS.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly DockSaaSDbContext _context;
    private readonly IServiceManager _serviceManager;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(
        DockSaaSDbContext context,
        IServiceManager serviceManager,
        ILogger<ServicesController> logger)
    {
        _context = context;
        _serviceManager = serviceManager;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        return Guid.Parse(tenantIdClaim ?? throw new UnauthorizedAccessException("Tenant ID not found"));
    }

    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<ServiceDefinitionDto>>> GetServiceDefinitions()
    {
        try
        {
            var definitions = await _context.ServiceDefinitions
                .Where(sd => sd.IsActive)
                .Select(sd => new ServiceDefinitionDto(
                    sd.Id,
                    sd.Name,
                    sd.Type,
                    sd.Description,
                    sd.IconUrl,
                    sd.ConfigurationSchema,
                    sd.DefaultConfiguration,
                    sd.IsActive
                ))
                .ToListAsync();

            return Ok(definitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service definitions");
            return StatusCode(500, "Failed to get service definitions");
        }
    }

    [HttpGet("instances")]
    public async Task<ActionResult<IEnumerable<ServiceInstanceDto>>> GetServiceInstances()
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var instances = await _context.ServiceInstances
                .Include(si => si.ServiceDefinition)
                .Where(si => si.TenantId == tenantId)
                .Select(si => new ServiceInstanceDto(
                    si.Id,
                    si.Name,
                    si.ServiceDefinitionId,
                    si.ServiceDefinition!.Name,
                    si.ServiceDefinition.Type,
                    si.TenantId,
                    si.Configuration,
                    si.Status,
                    si.EndpointUrl,
                    si.ApiKey,
                    si.UsageQuota,
                    si.CurrentUsage,
                    si.CreatedAt,
                    si.UpdatedAt,
                    si.LastAccessedAt
                ))
                .ToListAsync();

            return Ok(instances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service instances");
            return StatusCode(500, "Failed to get service instances");
        }
    }

    [HttpGet("instances/{id}")]
    public async Task<ActionResult<ServiceInstanceDto>> GetServiceInstance(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var instance = await _context.ServiceInstances
                .Include(si => si.ServiceDefinition)
                .FirstOrDefaultAsync(si => si.Id == id && si.TenantId == tenantId);

            if (instance == null)
                return NotFound();

            var dto = new ServiceInstanceDto(
                instance.Id,
                instance.Name,
                instance.ServiceDefinitionId,
                instance.ServiceDefinition!.Name,
                instance.ServiceDefinition.Type,
                instance.TenantId,
                instance.Configuration,
                instance.Status,
                instance.EndpointUrl,
                instance.ApiKey,
                instance.UsageQuota,
                instance.CurrentUsage,
                instance.CreatedAt,
                instance.UpdatedAt,
                instance.LastAccessedAt
            );

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service instance {Id}", id);
            return StatusCode(500, "Failed to get service instance");
        }
    }

    [HttpPost("instances")]
    public async Task<ActionResult<ServiceInstanceDto>> CreateServiceInstance([FromBody] CreateServiceInstanceRequest request)
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            // Check if service definition exists
            var serviceDefinition = await _context.ServiceDefinitions
                .FirstOrDefaultAsync(sd => sd.Id == request.ServiceDefinitionId && sd.IsActive);

            if (serviceDefinition == null)
                return BadRequest("Invalid service definition");

            // Check if name is unique within tenant
            var existingInstance = await _context.ServiceInstances
                .AnyAsync(si => si.Name == request.Name && si.TenantId == tenantId);

            if (existingInstance)
                return BadRequest("Service instance name already exists");

            var serviceInstance = new ServiceInstance
            {
                Name = request.Name,
                ServiceDefinitionId = request.ServiceDefinitionId,
                ServiceDefinition = serviceDefinition,
                TenantId = tenantId,
                Configuration = request.Configuration,
                UsageQuota = request.UsageQuota
            };

            var createdInstance = await _serviceManager.CreateServiceInstanceAsync(serviceInstance);

            var dto = new ServiceInstanceDto(
                createdInstance.Id,
                createdInstance.Name,
                createdInstance.ServiceDefinitionId,
                createdInstance.ServiceDefinition!.Name,
                createdInstance.ServiceDefinition.Type,
                createdInstance.TenantId,
                createdInstance.Configuration,
                createdInstance.Status,
                createdInstance.EndpointUrl,
                createdInstance.ApiKey,
                createdInstance.UsageQuota,
                createdInstance.CurrentUsage,
                createdInstance.CreatedAt,
                createdInstance.UpdatedAt,
                createdInstance.LastAccessedAt
            );

            return CreatedAtAction(nameof(GetServiceInstance), new { id = dto.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create service instance");
            return StatusCode(500, "Failed to create service instance");
        }
    }

    [HttpPut("instances/{id}")]
    public async Task<ActionResult<ServiceInstanceDto>> UpdateServiceInstance(Guid id, [FromBody] UpdateServiceInstanceRequest request)
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var instance = await _context.ServiceInstances
                .Include(si => si.ServiceDefinition)
                .FirstOrDefaultAsync(si => si.Id == id && si.TenantId == tenantId);

            if (instance == null)
                return NotFound();

            if (!string.IsNullOrEmpty(request.Name))
            {
                // Check if new name is unique within tenant
                var existingInstance = await _context.ServiceInstances
                    .AnyAsync(si => si.Name == request.Name && si.TenantId == tenantId && si.Id != id);

                if (existingInstance)
                    return BadRequest("Service instance name already exists");

                instance.Name = request.Name;
            }

            if (!string.IsNullOrEmpty(request.Configuration))
                instance.Configuration = request.Configuration;

            if (!string.IsNullOrEmpty(request.Status))
                instance.Status = request.Status;

            if (request.UsageQuota.HasValue)
                instance.UsageQuota = request.UsageQuota.Value;

            var updatedInstance = await _serviceManager.UpdateServiceInstanceAsync(instance);

            var dto = new ServiceInstanceDto(
                updatedInstance.Id,
                updatedInstance.Name,
                updatedInstance.ServiceDefinitionId,
                updatedInstance.ServiceDefinition!.Name,
                updatedInstance.ServiceDefinition.Type,
                updatedInstance.TenantId,
                updatedInstance.Configuration,
                updatedInstance.Status,
                updatedInstance.EndpointUrl,
                updatedInstance.ApiKey,
                updatedInstance.UsageQuota,
                updatedInstance.CurrentUsage,
                updatedInstance.CreatedAt,
                updatedInstance.UpdatedAt,
                updatedInstance.LastAccessedAt
            );

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update service instance {Id}", id);
            return StatusCode(500, "Failed to update service instance");
        }
    }

    [HttpDelete("instances/{id}")]
    public async Task<IActionResult> DeleteServiceInstance(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var instance = await _context.ServiceInstances
                .FirstOrDefaultAsync(si => si.Id == id && si.TenantId == tenantId);

            if (instance == null)
                return NotFound();

            var result = await _serviceManager.DeleteServiceInstanceAsync(id);

            if (!result)
                return StatusCode(500, "Failed to delete service instance");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete service instance {Id}", id);
            return StatusCode(500, "Failed to delete service instance");
        }
    }

    [HttpPost("instances/{id}/start")]
    public async Task<IActionResult> StartServiceInstance(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var instance = await _context.ServiceInstances
                .FirstOrDefaultAsync(si => si.Id == id && si.TenantId == tenantId);

            if (instance == null)
                return NotFound();

            var result = await _serviceManager.StartServiceAsync(id);

            if (!result)
                return StatusCode(500, "Failed to start service instance");

            return Ok(new { message = "Service instance started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start service instance {Id}", id);
            return StatusCode(500, "Failed to start service instance");
        }
    }

    [HttpPost("instances/{id}/stop")]
    public async Task<IActionResult> StopServiceInstance(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var instance = await _context.ServiceInstances
                .FirstOrDefaultAsync(si => si.Id == id && si.TenantId == tenantId);

            if (instance == null)
                return NotFound();

            var result = await _serviceManager.StopServiceAsync(id);

            if (!result)
                return StatusCode(500, "Failed to stop service instance");

            return Ok(new { message = "Service instance stopped successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop service instance {Id}", id);
            return StatusCode(500, "Failed to stop service instance");
        }
    }

    [HttpGet("instances/{id}/status")]
    public async Task<ActionResult<Dictionary<string, object>>> GetServiceInstanceStatus(Guid id)
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var instance = await _context.ServiceInstances
                .FirstOrDefaultAsync(si => si.Id == id && si.TenantId == tenantId);

            if (instance == null)
                return NotFound();

            var status = await _serviceManager.GetServiceStatusAsync(id);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service instance status {Id}", id);
            return StatusCode(500, "Failed to get service instance status");
        }
    }

    [HttpGet("instances/{id}/metrics")]
    public async Task<ActionResult<IEnumerable<ServiceMetricDto>>> GetServiceInstanceMetrics(Guid id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var tenantId = GetCurrentTenantId();

            var instance = await _context.ServiceInstances
                .FirstOrDefaultAsync(si => si.Id == id && si.TenantId == tenantId);

            if (instance == null)
                return NotFound();

            var query = _context.ServiceMetrics
                .Where(sm => sm.ServiceInstanceId == id);

            if (from.HasValue)
                query = query.Where(sm => sm.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(sm => sm.Timestamp <= to.Value);

            var metrics = await query
                .OrderByDescending(sm => sm.Timestamp)
                .Take(100)
                .Select(sm => new ServiceMetricDto(
                    sm.Id,
                    sm.ServiceInstanceId,
                    sm.MetricName,
                    sm.Value,
                    sm.Unit,
                    sm.Timestamp,
                    sm.Tags
                ))
                .ToListAsync();

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service instance metrics {Id}", id);
            return StatusCode(500, "Failed to get service instance metrics");
        }
    }
}