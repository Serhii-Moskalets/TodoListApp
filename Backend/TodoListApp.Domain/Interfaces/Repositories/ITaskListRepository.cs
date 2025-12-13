using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for working with <see cref="TaskListEntity"/>.
/// Provides methods for querying, creating, updating, and deleting task lists.
/// </summary>
public interface ITaskListRepository
{
    /// <summary>
    /// Retrieves a task list by its identifier.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="TaskListEntity"/> if found; otherwise, <c>null</c>.</returns>
    Task<TaskListEntity> GetByIdAsync(Guid taskListId, CancellationToken cancellationToken = default);

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
    /// Checks whether a task list exists by its identifier.
    /// </summary>
    /// <param name="todoListId">The identifier of the task list.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the task list exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(Guid todoListId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a specific user is the owner of a task list.
    /// </summary>
    /// <param name="todoListId">The identifier of the task list.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the user is the owner of the task list; otherwise, <c>false</c>.</returns>
    Task<bool> IsTodoListOwnerAsync(Guid todoListId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a task list with the specified title exists for a specific user.
    /// </summary>
    /// <param name="title">The title of the task list.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if a task list with the specified title exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByTitleAsync(string title, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new task list to the repository.
    /// </summary>
    /// <param name="todoList">The <see cref="TaskListEntity"/> to add.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(TaskListEntity todoList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task list in the repository.
    /// </summary>
    /// <param name="todoList">The <see cref="TaskListEntity"/> to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task UpdateAsync(TaskListEntity todoList, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task list by its identifier.
    /// </summary>
    /// <param name="taskListId">The identifier of the task list to delete.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    Task Delete(Guid taskListId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
    /// The result contains the number of state entries written to the database.
    /// </returns>
    Task<int> SavaChangesAsync(CancellationToken cancellationToken = default);
}
