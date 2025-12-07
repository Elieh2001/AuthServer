namespace AuthServer.Application.DTOs.Applications;

public class ApplicationDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ApplicationType { get; set; } = string.Empty;
    public string AllowedGrantTypes { get; set; } = string.Empty;
    public string AllowedScopes { get; set; } = string.Empty;
    public string RedirectUris { get; set; } = string.Empty;
    public string PostLogoutRedirectUris { get; set; } = string.Empty;
    public int? AccessTokenLifetimeSeconds { get; set; }
    public int? RefreshTokenLifetimeSeconds { get; set; }

    // Google Authentication
    public bool GoogleEnabled { get; set; }
    public string? GoogleClientId { get; set; }
    // Note: GoogleClientSecret is never returned for security reasons

    // Apple Authentication
    public bool AppleEnabled { get; set; }
    public string? AppleClientId { get; set; }
    public string? AppleTeamId { get; set; }
    public string? AppleKeyId { get; set; }
    // Note: ApplePrivateKey is never returned for security reasons

    // LinkedIn Authentication
    public bool LinkedInEnabled { get; set; }
    public string? LinkedInClientId { get; set; }
    // Note: LinkedInClientSecret is never returned for security reasons

    // Legacy Database Configuration
    public bool HasLegacyDatabase { get; set; }
    public string? LegacyDatabaseType { get; set; }
    public string? LegacyUserTableName { get; set; }
    public string? LegacyUserIdColumn { get; set; }
    public string? LegacyEmailColumn { get; set; }
    public string? LegacyUsernameColumn { get; set; }
    public string? LegacyPasswordColumn { get; set; }
    public string? LegacyPasswordHashAlgorithm { get; set; }
    // Note: LegacyDatabaseConnectionString is never returned for security reasons

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}