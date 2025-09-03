using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSBJr.container.DockSaaS.ApiService.Data;
using SSBJr.container.DockSaaS.ApiService.DTOs;
using SSBJr.container.DockSaaS.ApiService.Models;
using SSBJr.container.DockSaaS.ApiService.Services;

namespace SSBJr.container.DockSaaS.ApiService.Controllers;

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
                    Description = $"Tenant for {request.TenantName}",
                    Plan = "Free"
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
                FirstName = request.FirstName,
                LastName = request.LastName,
                TenantId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
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
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logged out successfully" });
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
}