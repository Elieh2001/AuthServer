using AuthServer.Application.DTOs.System;

namespace AuthServer.Application.Interfaces;

public interface IAuditService
{
    Task LogEventAsync(AuditLogDto auditLog);
    Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid? tenantId, Guid? userId, DateTime? from, DateTime? to, int page = 1, int pageSize = 50);

}
