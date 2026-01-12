using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.AddTagToTask;

/// <summary>
/// Represents a command to associate a tag with a task for a specific user.
/// </summary>
public record AddTagToTaskCommand(Guid TaskId, Guid UserId, Guid TagId)
    : ICommand;
