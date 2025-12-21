using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.CreateUserTaskAccess;

/// <summary>
/// Command to create a user-task access relationship.
/// </summary>
public record CreateUserTaskAccessCommand(Guid TaskId, Guid UserId)
    : ICommand;