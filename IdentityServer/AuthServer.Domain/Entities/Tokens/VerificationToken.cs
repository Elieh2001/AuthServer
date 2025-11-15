using AuthServer.Domain.Entities.Users;
using static AuthServer.Domain.Enumerations.Enums;

namespace AuthServer.Domain.Entities.Tokens;

public class VerificationToken : BaseEntity
{
    #region Properties

    public Guid UserId { get; set; }
    public virtual User User { get; set; }  =new User();

    public TokenType TokenType { get; set; }
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }

    #endregion

    #region Helpers

    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    public bool IsValid()
    {
        return !IsUsed && !IsExpired();
    }

    #endregion
}