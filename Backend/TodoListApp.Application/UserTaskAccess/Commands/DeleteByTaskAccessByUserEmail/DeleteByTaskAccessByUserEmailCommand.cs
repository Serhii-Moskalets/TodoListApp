using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteByTaskAccessByUserEmail;

/// <summary>
/// Command to delete a user-task access entry based on the task ID and the user's email.
/// </summary>
public record DeleteByTaskAccessByUserEmailCommand(Guid TaskId, string Email)
    : ICommand;
