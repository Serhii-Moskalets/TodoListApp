using System;

namespace TodoListApp.Api.DTOs.TaskList;

/// <summary>
/// Represents a request to update the title of an existing task list.
/// </summary>
public class UpdateTaskListRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the task list to be updated.
    /// </summary>
    public Guid TaskListId { get; set; }

    /// <summary>
    /// Gets or sets the new title for the task list.
    /// This value can be null if no title change is requested.
    /// </summary>
    public string? NewTitle { get; set; }
}
