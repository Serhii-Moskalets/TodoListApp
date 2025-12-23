namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents the base entity class that provides
/// a unique identifier for all domain entities.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the entity.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
