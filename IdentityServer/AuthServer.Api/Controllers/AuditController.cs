using AuthServer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.Controllers;

[ApiController]
[Route("api/Audit")]
[Authorize(Roles = "SuperAdmin,Admin,TenantAdmin")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditService auditService,
        ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get audit logs with optional filters
    /// </summary>
    [HttpGet("GetLogs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] Guid? tenantId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1)
        {
            return BadRequest(new { error = "Page must be greater than 0" });
        }

        if (pageSize < 1 || pageSize > 200)
        {
            return BadRequest(new { error = "Page size must be between 1 and 200" });
        }

        _logger.LogInformation(
            "Fetching audit logs - TenantId: {TenantId}, UserId: {UserId}, From: {From}, To: {To}, Page: {Page}, PageSize: {PageSize}",
            tenantId, userId, from, to, page, pageSize);

        var logs = await _auditService.GetAuditLogsAsync(tenantId, userId, from, to, page, pageSize);

        return Ok(new
        {
            data = logs,
            page,
            pageSize,
            filters = new
            {
                tenantId,
                userId,
                from,
                to
            }
        });
    }

    /// <summary>
    /// Get audit logs for a specific tenant
    /// </summary>
    [HttpGet("GetByTenant")]
    public async Task<IActionResult> GetByTenant(
        [FromQuery] Guid tenantId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1)
        {
            return BadRequest(new { error = "Page must be greater than 0" });
        }

        if (pageSize < 1 || pageSize > 200)
        {
            return BadRequest(new { error = "Page size must be between 1 and 200" });
        }

        _logger.LogInformation(
            "Fetching audit logs for tenant: {TenantId}, From: {From}, To: {To}, Page: {Page}, PageSize: {PageSize}",
            tenantId, from, to, page, pageSize);

        var logs = await _auditService.GetAuditLogsAsync(tenantId, null, from, to, page, pageSize);

        return Ok(new
        {
            data = logs,
            tenantId,
            page,
            pageSize,
            filters = new
            {
                from,
                to
            }
        });
    }

    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    [HttpGet("GetByUser")]
    public async Task<IActionResult> GetByUser(
        [FromQuery] Guid userId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1)
        {
            return BadRequest(new { error = "Page must be greater than 0" });
        }

        if (pageSize < 1 || pageSize > 200)
        {
            return BadRequest(new { error = "Page size must be between 1 and 200" });
        }

        _logger.LogInformation(
            "Fetching audit logs for user: {UserId}, From: {From}, To: {To}, Page: {Page}, PageSize: {PageSize}",
            userId, from, to, page, pageSize);

        var logs = await _auditService.GetAuditLogsAsync(null, userId, from, to, page, pageSize);

        return Ok(new
        {
            data = logs,
            userId,
            page,
            pageSize,
            filters = new
            {
                from,
                to
            }
        });
    }
}
