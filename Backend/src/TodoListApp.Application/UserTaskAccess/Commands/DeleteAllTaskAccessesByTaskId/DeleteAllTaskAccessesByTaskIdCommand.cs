using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteAllTaskAccessesByTaskId
{
    /// <summary>
    /// Represents a command to delete all user-task access entries for a specific task.
    /// </summary>
    public record DeleteAllTaskAccessesByTaskIdCommand(Guid TaskId, Guid UserId)
        : ICommand;
}
