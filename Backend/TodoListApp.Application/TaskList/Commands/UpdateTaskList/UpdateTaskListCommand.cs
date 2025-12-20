using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.TaskList.Commands.UpdateTaskList;

/// <summary>
/// Represents a command to update the title of a specific task list belonging to a user.
/// </summary>
public record UpdateTaskListCommand(Guid TaskListId, Guid UserId, string NewTitle)
    : ICommand { }
