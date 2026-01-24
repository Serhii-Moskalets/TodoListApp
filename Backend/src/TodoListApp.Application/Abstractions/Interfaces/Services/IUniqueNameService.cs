namespace TodoListApp.Application.Abstractions.Interfaces.Services;

/// <summary>
/// Defines a service for generating unique names by appending a suffix if the name already exists.
/// </summary>
public interface IUniqueNameService
{
    /// <summary>
    /// Generates a unique name based on the provided base name and a uniqueness check function.
    /// </summary>
    /// <param name="baseName">The original name to start with.</param>
    /// <param name="existsCheck">
    /// A delegate that checks if a given name already exists in the system.
    /// Returns <see langword="true"/> if the name is taken; otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a unique name (e.g., "Name", "Name (1)", "Name (2)").
    /// </returns>
    Task<string> GetUniqueNameAsync(
        string baseName,
        Func<string, CancellationToken, Task<bool>> existsCheck,
        CancellationToken cancellationToken);
}
