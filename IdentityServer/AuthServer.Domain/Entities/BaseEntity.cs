namespace AuthServer.Domain.Entities;

public abstract class BaseEntity
{
    #region Properties

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    #endregion
}