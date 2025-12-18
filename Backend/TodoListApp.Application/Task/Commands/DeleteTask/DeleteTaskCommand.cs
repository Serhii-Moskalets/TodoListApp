using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Task.Commands.DeleteTask;

/// <summary>
/// Represents a command to delete a specific task for a given user.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DeleteTaskCommand"/> class.
/// </remarks>
/// <param name="taskId">The unique identifier of the task to delete.</param>
/// <param name="userId">The unique identifier of the user who owns the task.</param>
public class DeleteTaskCommand(Guid taskId, Guid userId)
    : ICommand
{
    /// <summary>
    /// Gets the unique identifier of the task to delete.
    /// </summary>
    public Guid TaskId { get; } = taskId;

    /// <summary>
    /// Gets the unique identifier of the user who owns the task.
    /// </summary>
    public Guid UserId { get; } = userId;
}
