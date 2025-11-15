using static AuthServer.Domain.Enumerations.Enums;

namespace AuthServer.Domain.Entities.Tenants;

public class Tenant : SoftDeleteEntity
{
    #region Properties

    public string Name { get; set; }
    public string Subdomain { get; set; }
    public TenantStatus Status { get; set; }
    public string SubscriptionPlan { get; set; }

    // Password Policy Settings
    public int PasswordMinLength { get; set; } = 8;
    public bool PasswordRequireUppercase { get; set; } = true;
    public bool PasswordRequireLowercase { get; set; } = true;
    public bool PasswordRequireDigit { get; set; } = true;
    public bool PasswordRequireSpecialChar { get; set; } = true;

    // Security Settings
    public int SessionTimeoutMinutes { get; set; } = 60;
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int AccountLockoutDurationMinutes { get; set; } = 30;

    // Branding
    public string LogoUrl { get; set; }
    public string PrimaryColor { get; set; }

    // Navigation Properties
    public virtual ICollection<Application> Applications { get; set; }
    public virtual ICollection<User> Users { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    public virtual ICollection<TenantAdmin> TenantAdmins { get; set; }

    #endregion

    #region Constructors

    public Tenant()
    {
        Applications = new HashSet<Application>();
        Users = new HashSet<User>();
        RefreshTokens = new HashSet<RefreshToken>();
        TenantAdmins = new HashSet<TenantAdmin>();
        Status = TenantStatus.Active;
    }

    #endregion

}