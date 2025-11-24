
namespace AuthServer.Application.DTOs.System;

public class AuditLogDto
{
    public Guid? TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventCategory { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public Guid? ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalData { get; set; }
    public DateTime CreatedAt { get; set; }
}
