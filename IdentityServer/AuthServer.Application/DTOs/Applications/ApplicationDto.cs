namespace AuthServer.Application.DTOs.Applications;

public class ApplicationDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ApplicationType { get; set; } = string.Empty;
    public string AllowedGrantTypes { get; set; } = string.Empty;
    public string AllowedScopes { get; set; } = string.Empty;
    public string RedirectUris { get; set; } = string.Empty;
    public bool GoogleEnabled { get; set; }
    public bool AppleEnabled { get; set; }
    public bool LinkedInEnabled { get; set; }
    public bool HasLegacyDatabase { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
