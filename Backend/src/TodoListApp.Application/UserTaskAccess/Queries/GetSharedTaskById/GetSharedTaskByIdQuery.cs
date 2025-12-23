using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTaskById;

/// <summary>
/// Query used to retrieve a task shared with a specific user by task identifier.
/// </summary>
public record GetSharedTaskByIdQuery(Guid TaskId, Guid UserId)
    : IQuery<TaskDto>;
