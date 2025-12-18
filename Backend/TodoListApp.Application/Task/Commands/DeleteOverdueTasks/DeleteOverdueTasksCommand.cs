using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Task.Commands.DeleteOverdueTasks;

/// <summary>
/// Represents a command to delete overdue tasks for a given user and task list.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DeleteOverdueTasksCommand"/> class.
/// </remarks>
/// <param name="taskListId">The unique identifier of the task list.</param>
/// <param name="userId">The unique identifier of the user who owns the task.</param>
public class DeleteOverdueTasksCommand(Guid taskListId, Guid userId)
    : ICommand
{
    /// <summary>
    /// Gets the unique identifier of the task list.
    /// </summary>
    public Guid TaskListId { get; } = taskListId;

    /// <summary>
    /// Gets the unique identifier of the user who owns the task list.
    /// </summary>
    public Guid UserId { get; } = userId;
}
