namespace TodoListApp.Application.UserTaskAccess.Dto.ForUser;

/// <summary>
/// Data Transfer Object representing a comment on a task.
/// </summary>
public class CommentDto
{
    /// <summary>
    /// Gets the unique identifier of the comment.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the text content of the comment.
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Gets the date and time when the comment was created.
    /// </summary>
    public DateTime CreatedDate { get; init; }

    /// <summary>
    /// Gets the user who created the comment.
    /// </summary>
    public UserDto User { get; init; } = null!;
}
