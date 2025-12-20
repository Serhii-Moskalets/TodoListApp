using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Application.Task.Commands.ChangeTaskStatus;

/// <summary>
/// Represents a command to change the status of a specific task for a given user.
/// </summary>
/// <param name="taskId">The identifier of the task whose status will be changed.</param>
/// <param name="userId">The identifier of the user who owns the task.</param>
/// <param name="status">The new status to assign to the task.</param>
public class ChangeTaskStatusCommand(Guid taskId, Guid userId, StatusTask status)
    : ICommand
{
    /// <summary>
    /// Gets the identifier of the task whose status will be changed.
    /// </summary>
    public Guid TaskId { get; } = taskId;

    /// <summary>
    /// Gets the identifier of the user who owns the task.
    /// </summary>
    public Guid UserId { get; } = userId;

    /// <summary>
    /// Gets the new status to assign to the task.
    /// </summary>
    public StatusTask Status { get; } = status;
}
