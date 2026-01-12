using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask
{
    /// <summary>
    /// Represents a command to delete all user-task access entries for a specific task.
    /// </summary>
    public record DeleteTaskAccessesByTaskCommand(Guid TaskId, Guid UserId)
        : ICommand;
}
