using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.UserTaskAccess.Dto;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByTaskId;

/// <summary>
/// Represents a query to retrieve all users who have access to a specific task.
/// </summary>
public record GetSharedTasksByTaskIdQuery(Guid TaskId, Guid UserId)
    : IQuery<TaskAccessListDto>;