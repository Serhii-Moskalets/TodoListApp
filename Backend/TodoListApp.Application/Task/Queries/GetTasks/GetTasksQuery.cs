using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Application.Task.Queries.GetTasks;

/// <summary>
/// Represents a query to retrieve a list of tasks for a specific user with optional filters and sorting.
/// </summary>
/// <param name="UserId">The identifier of the user who owns the tasks.</param>
/// <param name="TaskListId">The identifier of the task list.</param>
/// <param name="TaskStatuses">Optional list of task statuses to filter the results.</param>
/// <param name="DueBefore">Optional date to filter tasks due before this date.</param>
/// <param name="DueAfter">Optional date to filter tasks due after this date.</param>
/// <param name="TaskSortBy">Optional sorting field for the tasks.</param>
/// <param name="Ascending">Indicates whether the sorting should be in ascending order.</param>
public sealed record GetTasksQuery(
    Guid UserId,
    Guid TaskListId,
    IReadOnlyCollection<StatusTask>? TaskStatuses,
    DateTime? DueBefore,
    DateTime? DueAfter,
    TaskSortBy? TaskSortBy,
    bool Ascending)
    : IQuery<IEnumerable<TaskDto>>;
