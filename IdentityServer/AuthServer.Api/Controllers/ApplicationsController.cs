using AuthServer.Application.DTOs.Applications;
using AuthServer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.Controllers;

[ApiController]
[Route("api/Application")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(
        IApplicationService applicationService,
        ILogger<ApplicationsController> logger)
    {
        _applicationService = applicationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new application
    /// </summary>
    [HttpPost("Add")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> Add([FromBody] CreateApplicationDto dto)
    {
        _logger.LogInformation("Creating new application: {ApplicationName}", dto.Name);

        var result = await _applicationService.CreateApplicationAsync(dto);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to create application. Reason: {Error}", result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("Application created successfully with ID: {ApplicationId}", result.Data.Id);
        return CreatedAtAction(nameof(GetById), new { applicationId = result.Data.Id }, result.Data);
    }

    /// <summary>
    /// Get application by ID
    /// </summary>
    [HttpGet("GetById")]
    public async Task<IActionResult> GetById([FromQuery] Guid applicationId)
    {
        _logger.LogInformation("Fetching application by ID: {ApplicationId}", applicationId);

        var result = await _applicationService.GetApplicationByIdAsync(applicationId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Application not found: {ApplicationId}. Reason: {Error}",
                applicationId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return NotFound(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get application by client ID
    /// </summary>
    [HttpGet("GetByClientId")]
    public async Task<IActionResult> GetByClientId([FromQuery] string clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            return BadRequest(new { error = "Client ID is required" });
        }

        _logger.LogInformation("Fetching application by client ID: {ClientId}", clientId);

        var result = await _applicationService.GetApplicationByClientIdAsync(clientId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Application not found with client ID: {ClientId}. Reason: {Error}",
                clientId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return NotFound(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all applications for a tenant
    /// </summary>
    [HttpGet("GetByTenant")]
    public async Task<IActionResult> GetByTenant([FromQuery] Guid tenantId)
    {
        _logger.LogInformation("Fetching applications for tenant: {TenantId}", tenantId);

        var result = await _applicationService.GetApplicationsByTenantAsync(tenantId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to fetch applications for tenant: {TenantId}. Reason: {Error}",
                tenantId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Update application information
    /// </summary>
    [HttpPost("Update")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> Update([FromQuery] Guid applicationId, [FromBody] UpdateApplicationDto dto)
    {
        _logger.LogInformation("Updating application: {ApplicationId}", applicationId);

        var result = await _applicationService.UpdateApplicationAsync(applicationId, dto);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update application: {ApplicationId}. Reason: {Error}",
                applicationId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("Application updated successfully: {ApplicationId}", applicationId);
        return Ok(result.Data);
    }

    /// <summary>
    /// Delete application
    /// </summary>
    [HttpPost("Delete")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> Delete([FromQuery] Guid applicationId)
    {
        _logger.LogInformation("Deleting application: {ApplicationId}", applicationId);

        var result = await _applicationService.DeleteApplicationAsync(applicationId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to delete application: {ApplicationId}. Reason: {Error}",
                applicationId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("Application deleted successfully: {ApplicationId}", applicationId);
        return Ok(new { message = "Application deleted successfully" });
    }

    /// <summary>
    /// Regenerate client secret for an application
    /// </summary>
    [HttpPost("RegenerateSecret")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> RegenerateSecret([FromQuery] Guid applicationId)
    {
        _logger.LogInformation("Regenerating client secret for application: {ApplicationId}", applicationId);

        var result = await _applicationService.RegenerateClientSecretAsync(applicationId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to regenerate client secret for application: {ApplicationId}. Reason: {Error}",
                applicationId, result.Errors?.FirstOrDefault() ?? "An error occurred");
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "An error occurred" });
        }

        _logger.LogInformation("Client secret regenerated successfully for application: {ApplicationId}",
            applicationId);
        return Ok(new
        {
            message = "Client secret regenerated successfully",
            clientSecret = result.Data
        });
    }

    /// <summary>
    /// Validate client credentials
    /// </summary>
    [HttpPost("ValidateCredentials")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateCredentials([FromBody] ValidateCredentialsDto dto)
    {
        if (string.IsNullOrEmpty(dto.ClientId) || string.IsNullOrEmpty(dto.ClientSecret))
        {
            return BadRequest(new { error = "Client ID and Client Secret are required" });
        }

        _logger.LogInformation("Validating client credentials for client ID: {ClientId}", dto.ClientId);

        var result = await _applicationService.ValidateClientCredentialsAsync(dto.ClientId, dto.ClientSecret);

        if (!result.IsSuccess || !result.Data)
        {
            _logger.LogWarning("Client credentials validation failed for client ID: {ClientId}", dto.ClientId);
            return Unauthorized(new { error = "Invalid client credentials" });
        }

        _logger.LogInformation("Client credentials validated successfully for client ID: {ClientId}", dto.ClientId);
        return Ok(new { valid = true, message = "Client credentials are valid" });
    }
}

// DTOs
public class ValidateCredentialsDto
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
