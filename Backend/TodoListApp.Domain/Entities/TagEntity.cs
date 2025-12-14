using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a tag that can be associated with tasks and owned by a user.
/// </summary>
[Table("Tags")]
public class TagEntity : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TagEntity"/> class.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <param name="userId">The ID of the user who created the tag.</param>
    public TagEntity(string name, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tag name cannot be empty", nameof(name));
        }

        this.Name = name.Trim();
        this.UserId = userId;
    }

    private TagEntity() { }

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    [Column("Name")]
    required public string Name { get; init; }

    /// <summary>
    /// Gets the ID of the user who owns this tag.
    /// </summary>
    [Column("User_Id")]
    required public Guid UserId { get; init; }

    /// <summary>
    /// Gets the user who owns this tag.
    /// </summary>
    public virtual UserEntity User { get; init; } = null!;

    /// <summary>
    /// Gets the collection of tasks associated with this tag.
    /// </summary>
    public virtual ICollection<TaskEntity> Tasks { get; private set; } = new HashSet<TaskEntity>();
}