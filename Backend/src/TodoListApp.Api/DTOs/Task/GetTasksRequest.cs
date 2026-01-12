using System;
using System.Collections.Generic;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Api.DTOs.Task;

/// <summary>
/// Represents the parameters for retrieving tasks with optional filtering, sorting, and date ranges.
/// </summary>
/// <param name="TaskListId">The ID of the task list from which to retrieve tasks.</param>
/// <param name="TaskStatuses">Optional. A collection of task statuses to filter the tasks by.</param>
/// <param name="DueBefore">Optional. Only include tasks with a due date before this value.</param>
/// <param name="DueAfter">Optional. Only include tasks with a due date after this value.</param>
/// <param name="TaskSortBy">Optional. Specifies the field by which to sort the tasks.</param>
/// <param name="Ascending">Specifies whether to sort in ascending order. Default is <c>true</c>.</param>
public record GetTasksRequest(
    Guid TaskListId,
    IReadOnlyCollection<StatusTask>? TaskStatuses = null,
    DateTime? DueBefore = null,
    DateTime? DueAfter = null,
    TaskSortBy? TaskSortBy = null,
    bool Ascending = true);
