using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;

namespace TodoListApp.Application.Tasks.Queries.GetTaskById;

/// <summary>
/// Query to retrieve a specific task by its ID for a given user.
/// </summary>
public sealed record GetTaskByIdQuery(Guid UserId, Guid TaskId)
    : IQuery<TaskDto>;
