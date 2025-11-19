namespace AuthServer.Application.DTOs.Applications;

public class UpdateApplicationDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RedirectUris { get; set; } = string.Empty;
    public string PostLogoutRedirectUris { get; set; } = string.Empty;
    public int? AccessTokenLifetimeSeconds { get; set; }
    public int? RefreshTokenLifetimeSeconds { get; set; }
    public bool? GoogleEnabled { get; set; }
    public string GoogleClientId { get; set; } = string.Empty;
    public string GoogleClientSecret { get; set; } = string.Empty;
    public bool? AppleEnabled { get; set; }
    public string AppleClientId { get; set; } = string.Empty;
    public bool? LinkedInEnabled { get; set; }
    public string LinkedInClientId { get; set; } = string.Empty;
    public string LinkedInClientSecret { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
}
