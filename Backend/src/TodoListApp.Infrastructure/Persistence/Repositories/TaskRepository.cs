using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TinyResult;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;

namespace TodoListApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository responsible for managing <see cref="TaskEntity"/> instances.
/// Provides methods for querying, paginating, filtering, and managing tasks,
/// including overdue tasks and task tags.
/// </summary>
public class TaskRepository(TodoListAppDbContext context)
   : BaseRepository<TaskEntity>(context), ITaskRepository
{
    /// <summary>
    /// Counts the number of overdue tasks for a specific user within a specific task list.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="taskListId">The identifier of the task list.</param>
    /// <param name="now">The current date and time used to determine overdue tasks.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of overdue tasks.</returns>
    public async Task<int> CountOverdueTasksAsync(
        Guid userId,
        Guid taskListId,
        DateTime now,
        CancellationToken cancellationToken = default)
        => await this.DbSet
            .Where(x => x.OwnerId == userId && x.TaskListId == taskListId && x.DueDate < now)
            .CountAsync(cancellationToken);

    /// <summary>
    /// Deletes all overdue tasks for a specific user within a specific task list.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="taskListId">The identifier of the task list.</param>
    /// <param name="now">The current date and time used to determine overdue tasks.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    /// <remarks>
    /// Changes are not automatically saved. Call SaveChangesAsync in service if needed.
    /// </remarks>
    public async Task DeleteOverdueTasksAsync(
        Guid userId,
        Guid taskListId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var overdueTasks = await this.DbSet
            .Where(x => x.OwnerId == userId && x.TaskListId == taskListId && x.DueDate < now)
            .ToListAsync(cancellationToken);

        this.DbSet.RemoveRange(overdueTasks);
    }

    /// <summary>
    /// Checks whether a specific task exists for a given user.
    /// </summary>
    /// <param name="taskId">The identifier of the task.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the task exists for the user; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsForUserAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking().AnyAsync(x => x.Id == taskId && x.OwnerId == userId, cancellationToken);

    /// <summary>
    /// Retrieves tasks for a user within a specific task list, optionally filtered by statuses, due dates, and sorting.
    /// Includes Tag and Comments related entities.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="taskListId">The To-Do list identifier.</param>
    /// <param name="now">The current date and time used to determine overdue tasks.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of overdue <see cref="TaskEntity"/> instances.</returns>
    public async Task<IReadOnlyCollection<TaskEntity>> GetOverdueTasksAsync(
        Guid userId,
        Guid taskListId,
        DateTime now,
        CancellationToken cancellationToken = default)
        => await this.DbSet
            .AsNoTracking()
            .Where(x => x.OwnerId == userId && x.TaskListId == taskListId && x.DueDate < now)
            .OrderBy(x => x.CreatedDate)
            .Include(x => x.Tag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.User)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Retrieves a paginated list of tasks for a user within a specific task list, optionally filtered by a predicate.
    /// Includes Tag and Comments related entities.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="taskListId">The To-Do list identifier.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="filter">An optional filter expression.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A tuple containing a read-only collection of <see cref="TaskEntity"/> items and the total count of matching tasks.
    /// </returns>
    public async Task<(IReadOnlyCollection<TaskEntity> Items, int TotalCount)> GetPaginatedAsync(
        Guid userId,
        Guid taskListId,
        int page,
        int pageSize,
        Expression<Func<TaskEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = this.DbSet.AsNoTracking()
            .Where(x => x.OwnerId == userId && x.TaskListId == taskListId);

        if (filter != null)
        {
            query = query.Where(filter);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.Tag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.User)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Retrieves a task entity by its identifier for a specific user.
    /// Includes the Tag and Comments (with User) related entities.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="userId">The unique identifier of the user who owns the task.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>
    /// The task entity with the specified ID for the given user, or <c>null</c> if not found.
    /// </returns>
    public async Task<TaskEntity?> GetTaskByIdForUserAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await this.DbSet
            .Include(x => x.Tag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == taskId && x.OwnerId == userId, cancellationToken);
    }

    /// <summary>
    /// Retrieves tasks for a user within a specific task list, optionally filtered by statuses, due dates, and sorting.
    /// Includes Tag and Comments related entities.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="todoListId">The identifier of the task list.</param>
    /// <param name="statuses">Optional collection of task statuses to filter by.</param>
    /// <param name="dueBefore">Optional due date upper limit.</param>
    /// <param name="dueAfter">Optional due date lower limit.</param>
    /// <param name="sortBy">Optional property name to sort by.</param>
    /// <param name="ascending">Whether to sort in ascending order. Defaults to <c>true</c>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="TaskEntity"/> instances matching the specified criteria.</returns>
    public async Task<IReadOnlyCollection<TaskEntity>> GetTasksAsync(
        Guid userId,
        Guid todoListId,
        IReadOnlyCollection<StatusTask>? statuses = null,
        DateTime? dueBefore = null,
        DateTime? dueAfter = null,
        TaskSortBy? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        var tasksQuery = this.DbSet.AsNoTracking().Where(x => x.OwnerId == userId && x.TaskListId == todoListId);

        if (statuses is { Count: > 0 })
        {
            tasksQuery = tasksQuery.Where(x => statuses.Contains(x.Status));
        }

        if (dueAfter.HasValue)
        {
            tasksQuery = tasksQuery.Where(x => x.DueDate >= dueAfter);
        }

        if (dueBefore.HasValue)
        {
            tasksQuery = tasksQuery.Where(x => x.DueDate <= dueBefore);
        }

        tasksQuery = sortBy switch
        {
            TaskSortBy.CreatedDate => ascending
                ? tasksQuery.OrderBy(t => t.CreatedDate)
                : tasksQuery.OrderByDescending(t => t.CreatedDate),

            TaskSortBy.DueDate => ascending
                ? tasksQuery.OrderBy(t => t.DueDate)
                : tasksQuery.OrderByDescending(t => t.DueDate),

            TaskSortBy.Title => ascending
                ? tasksQuery.OrderBy(t => t.Title)
                : tasksQuery.OrderByDescending(t => t.Title),

            TaskSortBy.Status => ascending
                ? tasksQuery.OrderBy(t => t.Status)
                : tasksQuery.OrderByDescending(t => t.Status),

            _ => tasksQuery,
        };

        return await tasksQuery
            .Include(x => x.Tag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.User)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Searches tasks by title for a specific user.
    /// Includes Tag and Comments related entities.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="searchText">The text to search in task titles.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="TaskEntity"/> instances whose titles match the search text.</returns>
    public async Task<IReadOnlyCollection<TaskEntity>> SearchByTitleAsync(Guid userId, string searchText, CancellationToken cancellationToken = default)
        => await this.DbSet
            .Where(x => x.OwnerId == userId && EF.Functions.Like(x.Title, $"%{searchText}%"))
            .OrderBy(x => x.CreatedDate)
            .Include(x => x.Tag)
            .Include(x => x.Comments)
                .ThenInclude(c => c.User)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Checks if a specific user is the owner of a task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that returns <c>true</c> if the user is the owner of the task; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> IsTaskOwnerAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking().AnyAsync(x => x.Id == taskId && x.OwnerId == userId, cancellationToken);
}
