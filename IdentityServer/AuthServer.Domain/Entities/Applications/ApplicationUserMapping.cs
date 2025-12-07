using AuthServer.Domain.Entities.Users;

namespace AuthServer.Domain.Entities.Applications;

public class ApplicationUserMapping : BaseEntity
{
    #region Properties

    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    public Guid ApplicationId { get; set; }
    public virtual Application Application { get; set; }

    // The user ID in the legacy application's database
    public string LegacyUserId { get; set; } = string.Empty;

    // Application-specific metadata (roles, permissions, etc.) as JSON
    public string Metadata { get; set; } = string.Empty;

    #endregion
}