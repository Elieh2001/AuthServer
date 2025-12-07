namespace AuthServer.Application.DTOs.Applications;

public class UpdateApplicationDto
{
    // Basic Information
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? RedirectUris { get; set; }
    public string? PostLogoutRedirectUris { get; set; }
    public int? AccessTokenLifetimeSeconds { get; set; }
    public int? RefreshTokenLifetimeSeconds { get; set; }

    // Google Authentication
    public bool? GoogleEnabled { get; set; }
    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecret { get; set; }

    // Apple Authentication
    public bool? AppleEnabled { get; set; }
    public string? AppleClientId { get; set; }
    public string? AppleTeamId { get; set; }
    public string? AppleKeyId { get; set; }
    public string? ApplePrivateKey { get; set; }

    // LinkedIn Authentication
    public bool? LinkedInEnabled { get; set; }
    public string? LinkedInClientId { get; set; }
    public string? LinkedInClientSecret { get; set; }

    // Legacy Database Configuration
    public bool? HasLegacyDatabase { get; set; }
    public string? LegacyDatabaseConnectionString { get; set; }
    public string? LegacyDatabaseType { get; set; }
    public string? LegacyUserTableName { get; set; }
    public string? LegacyUserIdColumn { get; set; }
    public string? LegacyEmailColumn { get; set; }
    public string? LegacyUsernameColumn { get; set; }
    public string? LegacyPasswordColumn { get; set; }
    public string? LegacyPasswordHashAlgorithm { get; set; }

    public bool? IsActive { get; set; }
}