using AuthServer.Domain.Entities.Tokens;
using static AuthServer.Domain.Enumerations.Enums;

namespace AuthServer.Domain.Entities.Applications;

public class Application : TenantEntity
{
    #region Properties

    public string Name { get; set; }
    public string Description { get; set; }

    // OAuth Client Credentials
    public string ClientId { get; set; }
    public string ClientSecretHash { get; set; }

    // Application Type
    public ApplicationType ApplicationType { get; set; }

    // OAuth Configuration
    public string AllowedGrantTypes { get; set; } // Comma-separated
    public string AllowedScopes { get; set; } // Comma-separated
    public string RedirectUris { get; set; } // JSON array
    public string PostLogoutRedirectUris { get; set; } // JSON array
    public string AllowedCorsOrigins { get; set; } // JSON array

    // Token Configuration (nullable = use global defaults)
    public int? AccessTokenLifetimeSeconds { get; set; }
    public int? RefreshTokenLifetimeSeconds { get; set; }
    public bool RefreshTokenRotationEnabled { get; set; } = true;
    public int? RefreshTokenAbsoluteLifetimeSeconds { get; set; }

    // External Providers - Google
    public bool GoogleEnabled { get; set; }
    public string GoogleClientId { get; set; }
    public string GoogleClientSecret { get; set; } // Encrypted

    // External Providers - Apple
    public bool AppleEnabled { get; set; }
    public string AppleClientId { get; set; }
    public string AppleTeamId { get; set; }
    public string AppleKeyId { get; set; }
    public string ApplePrivateKey { get; set; } // Encrypted

    // External Providers - LinkedIn
    public bool LinkedInEnabled { get; set; }
    public string LinkedInClientId { get; set; }
    public string LinkedInClientSecret { get; set; } // Encrypted

    // Legacy Database Configuration
    public bool HasLegacyDatabase { get; set; }
    public string LegacyDatabaseConnectionString { get; set; } // Encrypted
    public LegacyDatabaseType? LegacyDatabaseType { get; set; }
    public string LegacyUserTableName { get; set; }
    public string LegacyUserIdColumn { get; set; }
    public string LegacyEmailColumn { get; set; }
    public string LegacyPasswordColumn { get; set; }
    public PasswordHashAlgorithm? LegacyPasswordHashAlgorithm { get; set; }
    public string LegacyAdditionalColumnsMapping { get; set; } // JSON

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<ApplicationUserMapping> ApplicationUserMappings { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

    #endregion

    #region Constructor


    public Application()
    {
        ApplicationUserMappings = new HashSet<ApplicationUserMapping>();
        RefreshTokens = new HashSet<RefreshToken>();
    }

    #endregion
}
