using AuthServer.Application.DTOs.Tenants;
using AuthServer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.Controllers;

[ApiController]
[Route("api/Tenant")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        ITenantService tenantService,
        ILogger<TenantsController> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    [HttpPost("Add")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Add([FromBody] CreateTenantDto dto)
    {
        _logger.LogInformation("Creating new tenant: {TenantName}", dto.Name);

        var result = await _tenantService.CreateTenantAsync(dto);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to create tenant. Reason: {Error}", result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("Tenant created successfully with ID: {TenantId}", result.Data.Id);
        return CreatedAtAction(nameof(GetById), new { tenantId = result.Data.Id }, result.Data);
    }

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    [HttpGet("GetById")]
    public async Task<IActionResult> GetById([FromQuery] Guid tenantId)
    {
        _logger.LogInformation("Fetching tenant by ID: {TenantId}", tenantId);

        var result = await _tenantService.GetTenantByIdAsync(tenantId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Tenant not found: {TenantId}. Reason: {Error}", tenantId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return NotFound(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get tenant by subdomain
    /// </summary>
    [HttpGet("GetBySubdomain")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySubdomain([FromQuery] string subdomain)
    {
        if (string.IsNullOrEmpty(subdomain))
        {
            return BadRequest(new { error = "Subdomain is required" });
        }

        _logger.LogInformation("Fetching tenant by subdomain: {Subdomain}", subdomain);

        var result = await _tenantService.GetTenantBySubdomainAsync(subdomain);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Tenant not found with subdomain: {Subdomain}. Reason: {Error}",
                subdomain, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return NotFound(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all tenants
    /// </summary>
    [HttpGet("GetAll")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Fetching all tenants");

        var result = await _tenantService.GetAllTenantsAsync();

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to fetch tenants. Reason: {Error}", result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        return Ok(result.Data);
    }

    
    [HttpGet("debug")]
    public IActionResult DebugRoles()
    {
        return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
    }


    /// <summary>
    /// Update tenant information
    /// </summary>
    [HttpPost("Update")]
    [Authorize(Roles = "SuperAdmin,Admin,TenantAdmin")]
    public async Task<IActionResult> Update([FromQuery] Guid tenantId, [FromBody] UpdateTenantDto dto)
    {
        _logger.LogInformation("Updating tenant: {TenantId}", tenantId);

        var result = await _tenantService.UpdateTenantAsync(tenantId, dto);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update tenant: {TenantId}. Reason: {Error}", tenantId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("Tenant updated successfully: {TenantId}", tenantId);
        return Ok(result.Data);
    }

    /// <summary>
    /// Delete tenant
    /// </summary>
    [HttpPost("Delete")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete([FromQuery] Guid tenantId)
    {
        _logger.LogInformation("Deleting tenant: {TenantId}", tenantId);

        var result = await _tenantService.DeleteTenantAsync(tenantId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to delete tenant: {TenantId}. Reason: {Error}", tenantId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("Tenant deleted successfully: {TenantId}", tenantId);
        return Ok(new { message = "Tenant deleted successfully" });
    }

    /// <summary>
    /// Suspend tenant
    /// </summary>
    [HttpPost("Suspend")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Suspend([FromQuery] Guid tenantId)
    {
        _logger.LogInformation("Suspending tenant: {TenantId}", tenantId);

        var result = await _tenantService.SuspendTenantAsync(tenantId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to suspend tenant: {TenantId}. Reason: {Error}", tenantId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("Tenant suspended successfully: {TenantId}", tenantId);
        return Ok(new { message = "Tenant suspended successfully" });
    }

    /// <summary>
    /// Activate tenant
    /// </summary>
    [HttpPost("Activate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Activate([FromQuery] Guid tenantId)
    {
        _logger.LogInformation("Activating tenant: {TenantId}", tenantId);

        var result = await _tenantService.ActivateTenantAsync(tenantId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to activate tenant: {TenantId}. Reason: {Error}", tenantId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("Tenant activated successfully: {TenantId}", tenantId);
        return Ok(new { message = "Tenant activated successfully" });
    }
}
