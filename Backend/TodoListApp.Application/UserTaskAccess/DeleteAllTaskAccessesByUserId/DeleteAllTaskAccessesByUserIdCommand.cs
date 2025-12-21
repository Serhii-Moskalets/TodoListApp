using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.DeleteAllTaskAccessesByUserId;

/// <summary>
/// Represents a command to delete all user-task access entries for a specific user.
/// </summary>
public record DeleteAllTaskAccessesByUserIdCommand(Guid UserId)
    : ICommand;
