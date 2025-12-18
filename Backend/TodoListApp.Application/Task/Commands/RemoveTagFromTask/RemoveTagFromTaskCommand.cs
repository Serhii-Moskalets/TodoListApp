using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Task.Commands.RemoveTagFromTask;

/// <summary>
/// Represents a command to remove a tag from a specific task.
/// </summary>
public class RemoveTagFromTaskCommand : ICommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveTagFromTaskCommand"/> class.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task from which the tag will be removed.</param>
    /// <param name="userId">The unique identifier of the user who owns the task.</param>
    public RemoveTagFromTaskCommand(Guid taskId, Guid userId)
    {
        this.TaskId = taskId;
        this.UserId = userId;
    }

    /// <summary>
    /// Gets the unique identifier of the task from which the tag will be removed.
    /// </summary>
    public Guid TaskId { get; }

    /// <summary>
    /// Gets the unique identifier of the user who owns the task.
    /// </summary>
    public Guid UserId { get; }
}
