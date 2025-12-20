using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;

namespace TodoListApp.Application.Task.Queries.GetTaskById;

/// <summary>
/// Represents a query to retrieve a specific task by its identifier for a given user.
/// </summary>
/// <param name="UserId">The identifier of the user who owns the task.</param>
/// <param name="TaskId">The identifier of the task to retrieve.</param>
public sealed record GetTaskByIdQuery(Guid UserId, Guid TaskId)
    : IQuery<TaskDto>;
