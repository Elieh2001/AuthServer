using AuthServer.Application.DTOs.Users;
using AuthServer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.Controllers;

[ApiController]
[Route("api/User")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Debug endpoint to view claims
    /// </summary>
    [HttpGet("debug")]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var isInAdminRole = User.IsInRole("Admin");
        var isInSuperAdminRole = User.IsInRole("SuperAdmin");
        var isInTenantAdminRole = User.IsInRole("TenantAdmin");

        return Ok(new
        {
            claims,
            roles = new
            {
                isAdmin = isInAdminRole,
                isSuperAdmin = isInSuperAdminRole,
                isTenantAdmin = isInTenantAdminRole
            },
            identity = new
            {
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                name = User.Identity?.Name,
                authenticationType = User.Identity?.AuthenticationType
            }
        });
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost("Add")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> Add([FromBody] CreateUserDto dto)
    {
        _logger.LogInformation("Creating new user with email: {Email}", dto.Email);

        var result = await _userService.CreateUserAsync(dto);

        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? "Failed to create user";
            _logger.LogWarning("Failed to create user. Reason: {Error}", error);
            return BadRequest(new { error });
        }

        _logger.LogInformation("User created successfully with ID: {UserId}", result.Data.Id);
        return CreatedAtAction(nameof(GetById), new { userId = result.Data.Id }, result.Data);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("GetById")]
    public async Task<IActionResult> GetById([FromQuery] Guid userId)
    {
        _logger.LogInformation("Fetching user by ID: {UserId}", userId);

        var result = await _userService.GetUserByIdAsync(userId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("User not found: {UserId}. Reason: {Error}", userId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return NotFound(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    [HttpGet("GetByEmail")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email, [FromQuery] Guid tenantId)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { error = "Email is required" });
        }

        _logger.LogInformation("Fetching user by email: {Email} for tenant: {TenantId}", email, tenantId);

        var result = await _userService.GetUserByEmailAsync(email, tenantId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("User not found with email: {Email}. Reason: {Error}", email, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return NotFound(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all users for a tenant with pagination
    /// </summary>
    [HttpGet("GetByTenant")]
    public async Task<IActionResult> GetByTenant(
        [FromQuery] Guid tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1)
        {
            return BadRequest(new { error = "Page must be greater than 0" });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "Page size must be between 1 and 100" });
        }

        _logger.LogInformation("Fetching users for tenant: {TenantId}, page: {Page}, pageSize: {PageSize}",
            tenantId, page, pageSize);

        var result = await _userService.GetUsersByTenantAsync(tenantId, page, pageSize);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to fetch users for tenant: {TenantId}. Reason: {Error}",
                tenantId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        return Ok(new
        {
            data = result.Data,
            page,
            pageSize
        });
    }

    /// <summary>
    /// Update user information
    /// </summary>
    [HttpPost("Update")]
    public async Task<IActionResult> Update([FromQuery] Guid userId, [FromBody] UpdateUserDto dto)
    {
        _logger.LogInformation("Updating user: {UserId}", userId);

        var result = await _userService.UpdateUserAsync(userId, dto);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update user: {UserId}. Reason: {Error}", userId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("User updated successfully: {UserId}", userId);
        return Ok(result.Data);
    }

    /// <summary>
    /// Delete user
    /// </summary>
    [HttpPost("Delete")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> Delete([FromQuery] Guid userId)
    {
        _logger.LogInformation("Deleting user: {UserId}", userId);

        var result = await _userService.DeleteUserAsync(userId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to delete user: {UserId}. Reason: {Error}", userId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("User deleted successfully: {UserId}", userId);
        return Ok(new { message = "User deleted successfully" });
    }

    /// <summary>
    /// Lock user account for specified duration
    /// </summary>
    [HttpPost("Lock")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> Lock([FromQuery] Guid userId, [FromBody] LockUserDto dto)
    {
        _logger.LogInformation("Locking user: {UserId} for {Duration} minutes", userId, dto.DurationMinutes);

        var result = await _userService.LockUserAsync(userId, dto.DurationMinutes);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to lock user: {UserId}. Reason: {Error}", userId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("User locked successfully: {UserId}", userId);
        return Ok(new { message = "User locked successfully" });
    }

    /// <summary>
    /// Unlock user account
    /// </summary>
    [HttpPost("Unlock")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> Unlock([FromQuery] Guid userId)
    {
        _logger.LogInformation("Unlocking user: {UserId}", userId);

        var result = await _userService.UnlockUserAsync(userId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to unlock user: {UserId}. Reason: {Error}", userId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("User unlocked successfully: {UserId}", userId);
        return Ok(new { message = "User unlocked successfully" });
    }

    /// <summary>
    /// Link external authentication provider to user
    /// </summary>
    [HttpPost("LinkProvider")]
    public async Task<IActionResult> LinkProvider([FromQuery] Guid userId, [FromBody] LinkExternalProviderDto dto)
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;

        // Only allow users to link providers to their own account unless admin
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        if (currentUserId != userId && !User.IsInRole("Admin") && !User.IsInRole("TenantAdmin"))
        {
            return Forbid();
        }

        _logger.LogInformation("Linking external provider {Provider} to user: {UserId}", dto.Provider, userId);

        var result = await _userService.LinkExternalProviderAsync(userId, dto);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to link provider for user: {UserId}. Reason: {Error}",
                userId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("External provider linked successfully for user: {UserId}", userId);
        return Ok(new { message = "External provider linked successfully" });
    }

    /// <summary>
    /// Unlink external authentication provider from user
    /// </summary>
    [HttpPost("UnlinkProvider")]
    public async Task<IActionResult> UnlinkProvider([FromQuery] Guid userId, [FromQuery] string provider)
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;

        // Only allow users to unlink providers from their own account unless admin
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        if (currentUserId != userId && !User.IsInRole("Admin") && !User.IsInRole("TenantAdmin"))
        {
            return Forbid();
        }

        _logger.LogInformation("Unlinking external provider {Provider} from user: {UserId}", provider, userId);

        var result = await _userService.UnlinkExternalProviderAsync(userId, provider);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to unlink provider for user: {UserId}. Reason: {Error}",
                userId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("External provider unlinked successfully for user: {UserId}", userId);
        return Ok(new { message = "External provider unlinked successfully" });
    }
}

// DTOs
public class LockUserDto
{
    public int DurationMinutes { get; set; }
}
