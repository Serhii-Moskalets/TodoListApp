namespace TodoListApp.Api.DTOs.Task;

/// <summary>
/// Represents the parameters for retrieving tasks by their title.
/// </summary>
/// <param name="Title">Optional. The title (or part of the title) to filter tasks by.</param>
public record GetTaskByTitleRequest(string? Title);