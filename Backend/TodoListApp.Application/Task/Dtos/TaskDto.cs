using TodoListApp.Domain.Enums;

namespace TodoListApp.Application.Task.Dtos;

/// <summary>
/// Data Transfer Object representing a task.
/// Used for transferring task data between layers (Application → API → UI).
/// </summary>
public class TaskDto
{
    /// <summary>
    /// Gets the unique identifier of the task.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the title of the task.
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// Gets the description of the task.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the date and time when the task was created.
    /// </summary>
    public DateTime CreatedDate { get; init; }

    /// <summary>
    /// Gets the due date of the task.
    /// </summary>
    public DateTime DueDate { get; init; }

    /// <summary>
    /// Gets the current status of the task.
    /// </summary>
    public StatusTask Status { get; init; }

    /// <summary>
    /// Gets the ID of the user who owns the task.
    /// </summary>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the ID of the task list this task belongs to.
    /// </summary>
    public Guid TodoListId { get; init; }

    /// <summary>
    /// Gets the tag associated with the task, if any.
    /// </summary>
    public TagDto? Tag { get; init; }

    /// <summary>
    /// Gets the collection of comments associated with the task.
    /// </summary>
    public IList<CommentDto> Comments { get; init; } = Array.Empty<CommentDto>();
}
