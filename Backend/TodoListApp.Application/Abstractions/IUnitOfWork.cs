namespace TodoListApp.Application.Abstractions;

/// <summary>
/// Represents a unit of work that groups one or more operations
/// to be committed to the database as a single transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
