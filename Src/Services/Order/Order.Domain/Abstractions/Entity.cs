namespace Order.Domain.Abstractions;

/// <summary>
/// Base class for all entities, providing identity and audit fields.
/// </summary>
/// <typeparam name="T">Type of the entity identifier.</typeparam>
public abstract class Entity<T> : IEntity<T>
{
    /// <summary>
    /// Unique identifier of the entity.
    /// </summary>
    public T Id { get; set; }

    /// <summary>
    /// Timestamp when the entity was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// User who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when the entity was last modified.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// User who last modified the entity.
    /// </summary>
    public string? LastModifiedBy { get; set; }
}
