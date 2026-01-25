using System.ComponentModel.DataAnnotations.Schema;

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
    [Column("id")]
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    [Column("created_date")]
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
}
