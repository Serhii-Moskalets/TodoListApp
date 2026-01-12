namespace TodoListApp.Application.Abstractions.Interfaces.Services;

/// <summary>
/// Provides functionality for generating a unique task list title for a specific user.
/// Ensures that the returned title does not conflict with existing task list titles
/// owned by the same user.
/// </summary>
public interface ITaskListNameUniquenessService
{
    /// <summary>
    /// Generates a unique task list title for a user.
    /// </summary>
    /// <param name="userId">The identifier of the task list owner.</param>
    /// <param name="title">The base title to make unique.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A unique task list title.</returns>
    Task<string> GetUniqueNameAsync(Guid userId, string title, CancellationToken cancellationToken);
}
