using AuthServer.Application.DTOs.System;
using AuthServer.Application.Interfaces;
using AuthServer.Domain.Entities.System;
using AuthServer.Domain.Interfaces;
using static AuthServer.Domain.Enumerations.Enums;

namespace AuthServer.Application.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogEventAsync(AuditLogDto auditLog)
    {
        var log = new AuditLog
        {
            TenantId = auditLog.TenantId,
            EventType = Enum.Parse<AuditEventType>(auditLog.EventType),
            EventCategory = Enum.Parse<AuditEventCategory>(auditLog.EventCategory),
            UserId = auditLog.UserId,
            UserEmail = auditLog.UserEmail,
            ApplicationId = auditLog.ApplicationId,
            ApplicationName = auditLog.ApplicationName,
            IpAddress = auditLog.IpAddress,
            UserAgent = auditLog.UserAgent,
            Success = auditLog.Success,
            ErrorMessage = auditLog.ErrorMessage,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid? tenantId, Guid? userId, DateTime? from, DateTime? to, int page = 1, int pageSize = 50)
    {
        var query = _unitOfWork.AuditLogs.Query();

        if (tenantId.HasValue)
            query = query.Where(a => a.TenantId == tenantId);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId);

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        var logs =  query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return logs.Select(l => new AuditLogDto
        {
            TenantId = l.TenantId,
            EventType = l.EventType.ToString(),
            EventCategory = l.EventCategory.ToString(),
            UserId = l.UserId,
            UserEmail = l.UserEmail,
            ApplicationId = l.ApplicationId,
            ApplicationName = l.ApplicationName,
            IpAddress = l.IpAddress,
            Success = l.Success,
            ErrorMessage = l.ErrorMessage,
            CreatedAt = l.CreatedAt
        });
    }
}
