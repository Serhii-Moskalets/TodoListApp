using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for working with <see cref="TaskListEntity"/>.
/// Provides methods for querying, creating, updating, and deleting task lists.
/// </summary>
public interface ITaskListRepository : IRepository<TaskListEntity>
{
    /// <summary>
    /// Retrieves all task lists owned by a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="TaskListEntity"/>.</returns>
    Task<IReadOnlyCollection<TaskListEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated collection of task lists for a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A tuple containing the paginated items and the total count of task lists.</returns>
    Task<(IReadOnlyCollection<TaskListEntity> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a task list by its unique identifier for a specific user.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list.</param>
    /// <param name="userId">The unique identifier of the user who owns the task list.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the <see cref="TaskListEntity"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<TaskListEntity?> GetByIdForUserAsync(Guid taskListId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a specific user is the owner of a task list.
    /// </summary>
    /// <param name="taskListId">The identifier of the task list.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the user is the owner of the task list; otherwise, <c>false</c>.</returns>
    Task<bool> IsTodoListOwnerAsync(Guid taskListId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a task list with the specified title exists for a specific user.
    /// </summary>
    /// <param name="title">The title of the task list.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if a task list with the specified title exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByTitleAsync(string title, Guid userId, CancellationToken cancellationToken = default);
}
