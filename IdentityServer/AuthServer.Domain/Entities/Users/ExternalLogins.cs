using static AuthServer.Domain.Enumerations.Enums;

namespace AuthServer.Domain.Entities.Users;

public class ExternalLogin : BaseEntity
{
    #region Properties

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = new User();

    public ExternalProvider Provider { get; set; }
    public string ProviderUserId { get; set; } = string.Empty;
    public string ProviderEmail { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;

    public DateTime LinkedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    #endregion
}