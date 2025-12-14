using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing <see cref="UserTaskAccessEntity"/>.
/// Provides methods for querying, adding, and deleting user task access records.
/// </summary>
public interface IUserTaskAccessRepository
{
    /// <summary>
    /// Adds a new user task access record.
    /// </summary>
    /// <param name="entity">The user task access entity to add.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(UserTaskAccessEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user task access record by task ID and user ID.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="UserTaskAccessEntity"/> if found; otherwise, <c>null</c>.</returns>
    Task<UserTaskAccessEntity?> GetByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all tasks shared with a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="TaskEntity"/> shared with the user.</returns>
    Task<IReadOnlyCollection<UserTaskAccessEntity>> GetSharedTasksForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user task access record exists for the given task and user.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the access record exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user task access record by task ID and user ID.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    Task DeleteByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all user task access records associated with a specific task.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    Task DeleteAllByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all user task access records associated with a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
