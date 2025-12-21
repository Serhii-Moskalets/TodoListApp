using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.DeleteUTAById;

/// <summary>
/// Represents a command to delete a user-task access entry by task and user IDs.
/// </summary>
public record DeleteUtaByIdCommand(Guid TaskId, Guid UserId)
    : ICommand;
