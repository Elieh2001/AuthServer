using static AuthServer.Domain.Enumerations.Enums;

namespace AuthServer.Domain.Entities.Tenants;

public class TenantAdmin : BaseEntity
{
    #region Properties

    public Guid? TenantId { get; set; } // NULL for super admins
    public virtual Tenant? Tenant { get; set; }

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public AdminRole Role { get; set; }

    // Security
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();
    public bool TwoFactorEnabled { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    #endregion
}

