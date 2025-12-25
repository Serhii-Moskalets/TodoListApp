using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Application.Tasks.Queries.GetTasks;

/// <summary>
/// Query to retrieve a list of tasks for a specific user with optional filters and sorting.
/// </summary>
public sealed record GetTasksQuery(
    Guid UserId,
    Guid TaskListId,
    IReadOnlyCollection<StatusTask>? TaskStatuses = null,
    DateTime? DueBefore = null,
    DateTime? DueAfter = null,
    TaskSortBy? TaskSortBy = null,
    bool Ascending = true)
    : IQuery<IEnumerable<TaskDto>>;
