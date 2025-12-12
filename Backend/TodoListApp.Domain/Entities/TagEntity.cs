using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a tag that can be associated with tasks and owned by a user.
/// </summary>
[Table("Tags")]
public class TagEntity
{
    /// <summary>
    /// Gets the unique identifier of the tag.
    /// </summary>
    [Column("Tag_Id")]
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the tag.
    /// </summary>
    [Column("Name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ID of the user who owns this tag.
    /// </summary>
    [Column("User_Id")]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user who owns this tag.
    /// </summary>
    public UserEntity User { get; set; } = null!;

    /// <summary>
    /// Gets the collection of tasks associated with this tag.
    /// </summary>
    public List<TaskEntity> Tasks { get; private set; } = new();
}