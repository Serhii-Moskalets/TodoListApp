using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Task.Commands.DeleteTask;

/// <summary>
/// Represents a command to delete a specific task for a given user.
/// </summary>
public class DeleteTaskCommand : ICommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskCommand"/> class.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task to delete.</param>
    /// <param name="userId">The unique identifier of the user who owns the task.</param>
    public DeleteTaskCommand(Guid taskId, Guid userId)
    {
        this.TaskId = taskId;
        this.UserId = userId;
    }

    /// <summary>
    /// Gets the unique identifier of the task to delete.
    /// </summary>
    public Guid TaskId { get; }

    /// <summary>
    /// Gets the unique identifier of the user who owns the task.
    /// </summary>
    public Guid UserId { get; }
}
