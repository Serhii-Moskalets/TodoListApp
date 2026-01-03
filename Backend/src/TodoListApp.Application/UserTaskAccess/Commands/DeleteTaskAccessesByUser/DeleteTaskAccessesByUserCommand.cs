using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;

/// <summary>
/// Represents a command to delete all user-task access entries for a specific user.
/// </summary>
public record DeleteTaskAccessesByUserCommand(Guid UserId)
    : ICommand;
