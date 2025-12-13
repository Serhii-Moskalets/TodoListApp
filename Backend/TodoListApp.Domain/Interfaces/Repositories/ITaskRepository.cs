using System.Linq.Expressions;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for working with <see cref="TaskEntity"/>.
/// Provides methods for querying, creating, updating, and deleting tasks.
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Retrieves tasks for a specific user and To-Do list with optional filtering and sorting.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="todoListId">The To-Do list identifier.</param>
    /// <param name="statuses">Optional collection of task statuses to filter by.</param>
    /// <param name="dueBefore">Optional upper bound for the task due date.</param>
    /// <param name="dueAfter">Optional lower bound for the task due date.</param>
    /// <param name="sortBy">Optional field name to sort by.</param>
    /// <param name="ascending">Indicates whether sorting should be ascending.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only collection of tasks.</returns>
    Task<IReadOnlyCollection<TaskEntity>> GetTasksAsync(
        Guid id,
        Guid todoListId,
        IReadOnlyCollection<StatusTask>? statuses = null,
        DateTime? dueBefore = null,
        DateTime? dueAfter = null,
        string? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all overdue tasks for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only collection of overdue tasks.</returns>
    Task<IReadOnlyCollection<TaskEntity>> GetOverdueTasksAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches tasks by title for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="searchText">The text to search for in task titles.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only collection of matching tasks.</returns>
    Task<IReadOnlyCollection<TaskEntity>> SearchByTitleAsync(
        Guid userId,
        string searchText,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a task by its identifier.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task if found; otherwise, <c>null</c>.</returns>
    Task<TaskEntity?> GetByIdAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a task exists for the specified user.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the task exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a task exists by its identifier.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the task exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts overdue tasks for a specific user and To-Do list.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="listId">The To-Do list identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of overdue tasks.</returns>
    Task<int> CountOverdueTasksAsync(
        Guid userId,
        Guid listId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new task.
    /// </summary>
    /// <param name="task">The task entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(
        TaskEntity task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    /// <param name="task">The task entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(
        TaskEntity task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task by its identifier.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all overdue tasks for a specific user and To-Do list.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="listId">The To-Do list identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteOverdueTasksAsync(
        Guid userId,
        Guid listId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
    /// The result contains the number of state entries written to the database.
    /// </returns>
    Task<int> SaveChangeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a tag from the specified task.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveTagAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves paginated tasks with optional filtering.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="todoListId">The To-Do list identifier.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the paginated items and the total count.
    /// </returns>
    Task<(IReadOnlyCollection<TaskEntity> Items, int TotalCount)> GetPaginatedAsync(
        Guid userId,
        Guid todoListId,
        int page,
        int pageSize,
        Expression<Func<TaskEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);
}