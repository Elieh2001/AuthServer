using static AuthServer.Domain.Enumerations.Enums;

namespace AuthServer.Domain.Entities.System;

public class AuditLog
{
    #region Properties

    public long AuditLogId { get; set; }
    public Guid? TenantId { get; set; }

    public AuditEventType EventType { get; set; }
    public AuditEventCategory EventCategory { get; set; }

    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;

    public Guid? ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public string AdditionalData { get; set; } = string.Empty; // JSON

    public DateTime CreatedAt { get; set; }

    #endregion
}
