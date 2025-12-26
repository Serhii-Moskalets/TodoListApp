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
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="text"/> is null, empty, or consists only of white-space characters.
    /// </exception>
    public CommentEntity(Guid taskId, Guid userId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Comment text cannot be empty", nameof(text));
        }

        this.TaskId = taskId;
        this.UserId = userId;
        this.Text = text.Trim();
        this.CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentEntity"/> class
    /// with the specified task ID, user ID, comment text, and the associated user entity.
    /// </summary>
    /// <param name="taskId">The ID of the task to which this comment belongs.</param>
    /// <param name="userId">The ID of the user who created the comment.</param>
    /// <param name="text">The text content of the comment. Cannot be null or empty.</param>
    /// <param name="user">The <see cref="UserEntity"/> representing the user who created the comment.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="text"/> is null, empty, or whitespace only.</exception>
    public CommentEntity(Guid taskId, Guid userId, string text, UserEntity user)
    : this(taskId, userId, text)
    {
        this.User = user;
    }

    private CommentEntity() { }

    /// <summary>
    /// Gets the text content of the comment.
    /// </summary>
    [Column("Text")]
    public string Text { get; private set; } = null!;

    /// <summary>
    /// Gets the date and time when the comment was created.
    /// </summary>
    [Column("Created_Date")]
    public DateTime CreatedDate { get; }

    /// <summary>
    /// Gets the ID of the task that this comment belongs to.
    /// </summary>
    [Column("Task_Id")]
    public Guid TaskId { get; }

    /// <summary>
    /// Gets the ID of the user who created the comment.
    /// </summary>
    [Column("User_Id")]
    public Guid UserId { get; }

    /// <summary>
    /// Gets the user who created the comment.
    /// </summary>
    public virtual UserEntity User { get; } = null!;

    /// <summary>
    /// Gets the task to which this comment belongs.
    /// </summary>
    public virtual TaskEntity Task { get; } = null!;

    /// <summary>
    /// Updates the text content of the comment.
    /// </summary>
    /// <param name="text">The new text content of the comment.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="text"/> is null, empty, or consists only of white-space characters.
    /// </exception>
    public void Update(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Comment text cannot be empty", nameof(text));
        }

        this.Text = text.Trim();
    }
}
