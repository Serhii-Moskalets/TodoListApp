using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Abstractions.Interfaces.Repositories;

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
    /// Checks whether a task list with the specified title exists for a specific user.
    /// </summary>
    /// <param name="title">The title of the task list.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if a task list with the specified title exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByTitleAsync(string title, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a task list entity by its identifier for a specific user.
    /// Includes the Tag and Comments (with User) related entities.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list.</param>
    /// <param name="userId">The unique identifier of the user who owns the task list.</param>
    /// <param name="asNoTracking">
    /// If <c>true</c>, the query will not track changes in the retrieved entity,
    /// which can improve performance for read-only operations.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>
    /// The task list entity with the specified ID for the given user, or <c>null</c> if not found.
    /// </returns>
    Task<TaskListEntity?> GetTaskListByIdForUserAsync(Guid taskListId, Guid userId, bool asNoTracking = true, CancellationToken cancellationToken = default);
}
