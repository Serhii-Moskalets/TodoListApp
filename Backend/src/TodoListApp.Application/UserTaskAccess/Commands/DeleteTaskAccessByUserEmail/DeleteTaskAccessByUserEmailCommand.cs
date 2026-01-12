using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

/// <summary>
/// Command to delete a user-task access entry based on the task ID and the user's email.
/// </summary>
public record DeleteTaskAccessByUserEmailCommand(Guid TaskId, Guid OwnerId, string? Email)
    : ICommand;
