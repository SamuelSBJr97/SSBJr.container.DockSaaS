using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSBJr.DockSaaS.ApiService.Data;
using SSBJr.DockSaaS.ApiService.DTOs;
using SSBJr.DockSaaS.ApiService.Models;
using SSBJr.DockSaaS.ApiService.Services;

namespace SSBJr.DockSaaS.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly DockSaaSDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService,
        DockSaaSDbContext context,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _userManager.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid credentials");

            // If tenant name is specified, verify user belongs to that tenant
            if (!string.IsNullOrEmpty(request.TenantName) && 
                user.Tenant.Name != request.TenantName)
                return Unauthorized("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            if (!user.IsActive)
                return Unauthorized("Account is deactivated");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Log successful login
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TenantId = user.TenantId,
                Action = "User Login",
                EntityType = "Authentication",
                Description = $"User {user.Email} logged in successfully",
                IpAddress = GetClientIpAddress(),
                UserAgent = Request.Headers["User-Agent"].FirstOrDefault(),
                Timestamp = DateTime.UtcNow,
                Level = "Info"
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            var userDto = new UserDto(
                user.Id,
                user.Email ?? "",
                user.FirstName,
                user.LastName,
                user.FullName,
                user.AvatarUrl,
                user.TenantId,
                user.Tenant.Name,
                user.CreatedAt,
                user.LastLoginAt,
                user.IsActive,
                roles
            );

            return Ok(new LoginResponse(
                token,
                refreshToken,
                DateTime.UtcNow.AddHours(24),
                userDto
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user {Email}", request.Email);
            return StatusCode(500, "Login failed");
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Check if tenant exists or create new one
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Name == request.TenantName);

            if (tenant == null)
            {
                tenant = new Tenant
                {
                    Name = request.TenantName,
                    Email = request.Email, // Set tenant email to the first user's email
                    Description = $"Tenant for {request.TenantName}",
                    Plan = "Free",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();
            }

            // Check user limits
            var currentUserCount = await _userManager.Users
                .CountAsync(u => u.TenantId == tenant.Id);

            if (currentUserCount >= tenant.UserLimit)
                return BadRequest("User limit exceeded for this tenant");

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true, // Auto-confirm for demo purposes
                FirstName = request.FirstName,
                LastName = request.LastName,
                TenantId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign Admin role if this is the first user in the tenant
            var roleToAssign = currentUserCount == 0 ? "Admin" : "User";
            await _userManager.AddToRoleAsync(user, roleToAssign);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Log successful registration
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TenantId = user.TenantId,
                Action = "User Registration",
                EntityType = "User",
                Description = $"User {user.Email} registered successfully with role {roleToAssign}",
                IpAddress = GetClientIpAddress(),
                UserAgent = Request.Headers["User-Agent"].FirstOrDefault(),
                Timestamp = DateTime.UtcNow,
                Level = "Info"
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            // Load tenant for response
            user.Tenant = tenant;

            var userDto = new UserDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.FullName,
                user.AvatarUrl,
                user.TenantId,
                user.Tenant.Name,
                user.CreatedAt,
                user.LastLoginAt,
                user.IsActive,
                roles
            );

            return Ok(new LoginResponse(
                token,
                refreshToken,
                DateTime.UtcNow.AddHours(24),
                userDto
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for user {Email}", request.Email);
            return StatusCode(500, "Registration failed");
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var tenantId = User.FindFirst("tenant_id")?.Value ?? User.FindFirst("TenantId")?.Value;

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(tenantId))
            {
                // Log logout
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse(userId),
                    TenantId = Guid.Parse(tenantId),
                    Action = "User Logout",
                    EntityType = "Authentication",
                    Description = $"User {userEmail} logged out",
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers["User-Agent"].FirstOrDefault(),
                    Timestamp = DateTime.UtcNow,
                    Level = "Info"
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }

            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(500, "Logout failed");
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto(
                user.Id,
                user.Email ?? "",
                user.FirstName,
                user.LastName,
                user.FullName,
                user.AvatarUrl,
                user.TenantId,
                user.Tenant.Name,
                user.CreatedAt,
                user.LastLoginAt,
                user.IsActive,
                roles
            );

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current user");
            return StatusCode(500, "Failed to get user information");
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Validate refresh token logic would go here
            // For now, we'll just validate the token exists and return a new token
            
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest("Invalid refresh token");

            // In a real implementation, you would validate the refresh token against stored tokens
            // and check if it's still valid, not expired, etc.
            
            var principal = _jwtService.ValidateToken(request.Token);
            if (principal == null)
                return Unauthorized("Invalid token");

            var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token claims");

            var user = await _userManager.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userIdClaim));

            if (user == null || !user.IsActive)
                return Unauthorized("User not found or inactive");

            var roles = await _userManager.GetRolesAsync(user);
            var newToken = _jwtService.GenerateToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            var userDto = new UserDto(
                user.Id,
                user.Email ?? "",
                user.FirstName,
                user.LastName,
                user.FullName,
                user.AvatarUrl,
                user.TenantId,
                user.Tenant.Name,
                user.CreatedAt,
                user.LastLoginAt,
                user.IsActive,
                roles
            );

            return Ok(new LoginResponse(
                newToken,
                newRefreshToken,
                DateTime.UtcNow.AddHours(24),
                userDto
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return StatusCode(500, "Token refresh failed");
        }
    }

    private string? GetClientIpAddress()
    {
        // Try to get real IP behind proxy
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            return forwarded.Split(',').FirstOrDefault()?.Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return Request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}

// Add refresh token request DTO
public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);