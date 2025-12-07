using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs.Applications;

public class CreateApplicationDto
{
    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public string ApplicationType { get; set; } = string.Empty;// Native, LegacyDatabase, Federated

    public string AllowedGrantTypes { get; set; } = "authorization_code,refresh_token";
    public string AllowedScopes { get; set; } = "openid,profile,email";
    public string RedirectUris { get; set; } = string.Empty;
    public string PostLogoutRedirectUris { get; set; } = string.Empty;

    // Token settings
    public int? AccessTokenLifetimeSeconds { get; set; }
    public int? RefreshTokenLifetimeSeconds { get; set; }

    // External providers
    public bool GoogleEnabled { get; set; }
    public string? GoogleClientId { get; set; } = string.Empty;
    public string? GoogleClientSecret { get; set; } = string.Empty;

    public bool AppleEnabled { get; set; }
    public string? AppleClientId { get; set; } = string.Empty;
    public string? AppleTeamId { get; set; } = string.Empty;
    public string? AppleKeyId { get; set; } = string.Empty;
    public string? ApplePrivateKey { get; set; } = string.Empty;

    public bool LinkedInEnabled { get; set; }
    public string? LinkedInClientId { get; set; } = string.Empty;
    public string? LinkedInClientSecret { get; set; } = string.Empty;

    // Legacy database
    public bool HasLegacyDatabase { get; set; }
    public string LegacyDatabaseConnectionString { get; set; } = string.Empty;
    public string LegacyDatabaseType { get; set; } = string.Empty;
    public string LegacyUserTableName { get; set; } = string.Empty;
    public string LegacyUserIdColumn { get; set; } = string.Empty;
    public string LegacyEmailColumn { get; set; } = string.Empty;
    public string LegacyUsernameColumn { get; set; } = string.Empty;
    public string LegacyPasswordColumn { get; set; } = string.Empty;
    public string LegacyPasswordHashAlgorithm { get; set; } = string.Empty;
}
