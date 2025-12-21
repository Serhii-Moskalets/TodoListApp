using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.UserTaskAccess.Dto.ForUser;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;

/// <summary>
/// Represents a query for retrieving all tasks that are shared with a specific user.
/// </summary>
public record GetSharedTasksByUserIdQuery(Guid UserId)
    : IQuery<IEnumerable<TaskDto>>;