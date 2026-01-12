using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;

/// <summary>
/// Command to delete all overdue tasks for a given task list and user.
/// </summary>
public record DeleteOverdueTasksCommand(Guid TaskListId, Guid UserId)
    : ICommand;
