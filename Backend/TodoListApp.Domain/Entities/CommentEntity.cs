using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a user's comment on a task.
/// </summary>
[Table("Comments")]
public class CommentEntity : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommentEntity"/> class.
    /// </summary>
    /// <param name="taskId">The ID of the task the comment belongs to.</param>
    /// <param name="userId">The ID of the user who created the comment.</param>
    /// <param name="text">The text content of the comment.</param>
    public CommentEntity(Guid taskId, Guid userId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Comment text cannot be empty", nameof(text));
        }

        this.TaskId = taskId;
        this.UserId = userId;
        this.Text = text;
        this.CreatedDate = DateTime.UtcNow;
    }

    private CommentEntity() { }

    /// <summary>
    /// Gets the text content of the comment.
    /// </summary>
    [Column("Text")]
    required public string Text { get; init; }

    /// <summary>
    /// Gets the date and time when the comment was created.
    /// </summary>
    [Column("Created_Date")]
    required public DateTime CreatedDate { get; init; }

    /// <summary>
    /// Gets the ID of the task that this comment belongs to.
    /// </summary>
    [Column("Task_Id")]
    required public Guid TaskId { get; init; }

    /// <summary>
    /// Gets the ID of the user who created the comment.
    /// </summary>
    [Column("User_Id")]
    required public Guid UserId { get; init; }

    /// <summary>
    /// Gets the user who created the comment.
    /// </summary>
    public virtual UserEntity User { get; init; } = null!;

    /// <summary>
    /// Gets the task to which this comment belongs.
    /// </summary>
    public virtual TaskEntity Task { get; init; } = null!;
}
