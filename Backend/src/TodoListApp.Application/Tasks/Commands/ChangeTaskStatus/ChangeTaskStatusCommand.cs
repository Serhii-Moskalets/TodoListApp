using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Application.Tasks.Commands.ChangeTaskStatus;

/// <summary>
/// Command to change the status of a specific task.
/// </summary>
public record ChangeTaskStatusCommand(Guid TaskId, Guid UserId, StatusTask Status)
    : ICommand;
