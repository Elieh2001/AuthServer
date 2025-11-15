using AuthServer.Domain.Entities.Applications;
using AuthServer.Domain.Entities.Users;

namespace AuthServer.Domain.Entities.Tokens;

public class RefreshToken : TenantEntity
{
    #region Properties

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = new User();

    public Guid ApplicationId { get; set; }
    public virtual Application Application { get; set; } = new Application();

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    // For rotation
    public Guid? ParentTokenId { get; set; }
    public virtual RefreshToken ParentToken { get; set; } = new RefreshToken();

    // Revocation
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string RevokedReason { get; set; } = string.Empty;
    public string RevokedByIp { get; set; } = string.Empty;

    // Token reuse detection
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }

    // Tracking
    public string CreatedByIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    #endregion

    #region Helpers

    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    public bool IsActive()
    {
        return !IsRevoked && !IsExpired();
    }

    #endregion
}
