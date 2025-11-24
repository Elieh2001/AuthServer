using AuthServer.Domain.Entities.Applications;
using AuthServer.Domain.Entities.Tokens;

namespace AuthServer.Domain.Entities.Users;

public class User : TenantEntity
{
    #region Properties

    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }

    // For native authentication (NULL if only external logins)
    public string PasswordHash { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
    public bool PhoneNumberVerified { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Security
    public Guid SecurityStamp { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool IsSystemAdmin { get; set; } = false;
    public string Roles { get; set; } = string.Empty;

    // Account Status
    public bool IsActive { get; set; } = true;
    public bool LockoutEnabled { get; set; } = true;
    public DateTime? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }

    // Tracking
    public DateTime? LastLoginAt { get; set; }
    public string LastLoginIp { get; set; } = string.Empty;

    // Navigation Properties
    public virtual ICollection<ExternalLogin> ExternalLogins { get; set; }
    public virtual ICollection<ApplicationUserMapping> ApplicationUserMappings { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    public virtual ICollection<VerificationToken> VerificationTokens { get; set; }

    #endregion

    #region Constructors

    public User()
    {
        ExternalLogins = new HashSet<ExternalLogin>();
        ApplicationUserMappings = new HashSet<ApplicationUserMapping>();
        RefreshTokens = new HashSet<RefreshToken>();
        VerificationTokens = new HashSet<VerificationToken>();
    }

    #endregion

    #region Helpers

    public string FullName => $"{FirstName} {LastName}".Trim();

    public bool IsLockedOut()
    {
        return LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    #endregion
}