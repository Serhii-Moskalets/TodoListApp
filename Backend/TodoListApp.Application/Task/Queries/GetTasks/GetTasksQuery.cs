using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Application.Task.Queries.GetTasks;

/// <summary>
/// Query to retrieve a list of tasks for a specific user with optional filters and sorting.
/// </summary>
public sealed record GetTasksQuery(
    Guid UserId,
    Guid TaskListId,
    IReadOnlyCollection<StatusTask>? TaskStatuses,
    DateTime? DueBefore,
    DateTime? DueAfter,
    TaskSortBy? TaskSortBy,
    bool Ascending)
    : IQuery<IEnumerable<TaskDto>>;
