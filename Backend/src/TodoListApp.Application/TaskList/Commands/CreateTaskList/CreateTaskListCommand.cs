using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.TaskList.Commands.CreateTaskList;

/// <summary>
/// Command representing the request to create a new task list for a user.
/// </summary>
public record CreateTaskListCommand(Guid UserId, string? Title)
    : ICommand;
