namespace TodoListApp.Api.DTOs.TaskList;

/// <summary>
/// Represents a request to create a new task list for a user.
/// </summary>
public class CreateTaskListRequest
{
    /// <summary>
    /// Gets or sets the title of the new task list.
    /// This value can be null if no title is provided.
    /// Validation should ensure that a non-empty title is provided when required.
    /// </summary>
    public string? Title { get; set; }
}
