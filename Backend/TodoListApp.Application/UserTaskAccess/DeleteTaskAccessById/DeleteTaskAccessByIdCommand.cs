using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.DeleteTaskAccessById;

/// <summary>
/// Represents a command to delete a user-task access entry by task and user IDs.
/// </summary>
public record DeleteTaskAccessByIdCommand(Guid TaskId, Guid UserId)
    : ICommand;
