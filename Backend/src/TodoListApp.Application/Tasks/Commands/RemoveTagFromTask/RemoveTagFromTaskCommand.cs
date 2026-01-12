using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.RemoveTagFromTask;

/// <summary>
/// Command to remove a tag from a specific task.
/// </summary>
public record RemoveTagFromTaskCommand(Guid TaskId, Guid UserId)
    : ICommand;
