using AuthServer.Domain.Entities.Tenants;

namespace AuthServer.Domain.Entities;

public abstract class TenantEntity : BaseEntity
{
    #region Properties

    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = new Tenant();
    
    #endregion
}
