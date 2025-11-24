using AuthServer.Domain.Entities.Applications;

namespace AuthServer.Domain.Entities.Tenants;

public class TenantApplication : BaseEntity
{
    #region Properties

    public Guid TenantId { get; set; }
    public Guid ApplicationId { get; set; }

    // Navigation Properties
    public virtual Tenant Tenant { get; set; }
    public virtual Application Application { get; set; }

    #endregion
}
