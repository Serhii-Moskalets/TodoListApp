using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a user's comment on a task.
/// </summary>
[Table("Comments")]
public class CommentEntity
{
    /// <summary>
    /// Gets the unique identifier of the comment.
    /// </summary>
    [Column("Comment_Id")]
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the text content of the comment.
    /// </summary>
    [Column("Text")]
    public string Text { get; set; } = null!;

    /// <summary>
    /// Gets the date and time when the comment was created.
    /// </summary>
    [Column("Created_Date")]
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the ID of the task that this comment belongs to.
    /// </summary>
    [Column("Task_Id")]
    [ForeignKey(nameof(Task))]
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created the comment.
    /// </summary>
    [Column("User_Id")]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user who created the comment.
    /// </summary>
    public UserEntity User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the task to which this comment belongs.
    /// </summary>
    public TaskEntity Task { get; set; } = null!;
}
