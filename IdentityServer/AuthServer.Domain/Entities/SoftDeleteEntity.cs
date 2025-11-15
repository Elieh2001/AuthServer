namespace AuthServer.Domain.Entities;

public abstract class SoftDeleteEntity : BaseEntity
{
    #region Properties

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    #endregion
}
