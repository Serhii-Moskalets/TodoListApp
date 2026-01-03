using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.UserTaskAccess.Dtos;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetTaskWithSharedUsers;

/// <summary>
/// Represents a query to retrieve all users who have access to a specific task.
/// </summary>
public record GetTaskWithSharedUsersQuery(Guid TaskId, Guid UserId)
    : IQuery<TaskAccessListDto>;