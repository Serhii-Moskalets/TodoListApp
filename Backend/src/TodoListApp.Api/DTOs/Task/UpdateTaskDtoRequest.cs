using System;

namespace TodoListApp.Api.DTOs.Task;

/// <summary>
/// Represents the data required to update an existing task.
/// </summary>
/// <param name="TaskId">The unique identifier of the task to update.</param>
/// <param name="Title">The new title of the task. Optional.</param>
/// <param name="Description">The new description of the task. Optional.</param>
/// <param name="DueDate">The new due date of the task. Optional.</param>
public record UpdateTaskDtoRequest(
    Guid TaskId,
    string? Title,
    string? Description,
    DateTime? DueDate);