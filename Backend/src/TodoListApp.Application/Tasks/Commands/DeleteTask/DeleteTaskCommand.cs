using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.DeleteTask;

/// <summary>
/// Command to delete a specific task for a given user.
/// </summary>
public record DeleteTaskCommand(Guid TaskId, Guid UserId)
    : ICommand;
