using System;

namespace TodoListApp.Api.DTOs.Task;

/// <summary>
/// Data Transfer Object for creating a new task.
/// </summary>
/// <param name="TaskListId">The identifier of the task list this task belongs to.</param>
/// <param name="Title">The title of the task. Can be null.</param>
/// <param name="DueDate">The due date of the task. Can be null.</param>
public record CreateTaskDtoRequest(
    Guid TaskListId,
    string? Title = null,
    DateTime? DueDate = null);