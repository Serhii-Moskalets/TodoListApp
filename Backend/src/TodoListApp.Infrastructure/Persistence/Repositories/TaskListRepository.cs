using Microsoft.EntityFrameworkCore;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;

namespace TodoListApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository responsible for data access operations related to <see cref="TaskListEntity"/>.
/// Provides methods for querying task lists by user, checking ownership,
/// pagination, and validating uniqueness constraints.
/// </summary>
public class TaskListRepository(TodoListAppDbContext context)
    : BaseRepository<TaskListEntity>(context), ITaskListRepository
{
    /// <summary>
    /// Determines whether a task list with the specified title already exists for a given user.
    /// </summary>
    /// <param name="title">The title of the task list to check.</param>
    /// <param name="userId">The identifier of the user who owns the task lists.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>
    /// <c>true</c> if a task list with the specified title exists for the user;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// /// <remarks>
    /// Uses `EF.Functions.Like` for real databases, and case-insensitive comparison for InMemory provider.
    /// </remarks>
    public async Task<bool> ExistsByTitleAsync(string title, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title of the task list cannot be empty.", nameof(title));
        }

        if (this.Context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            return await this.DbSet.AsNoTracking()
            .AnyAsync(x => x.Title.ToLowerInvariant() == title.ToLowerInvariant() && x.OwnerId == userId, cancellationToken);
        }

        return await this.DbSet.AsNoTracking()
            .AnyAsync(x => EF.Functions.Like(x.Title, title) && x.OwnerId == userId, cancellationToken);
    }

    /// <summary>
    /// Retrieves all task lists owned by the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user who owns the task lists.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>
    /// A read-only collection of <see cref="TaskListEntity"/> instances owned by the user.
    /// </returns>
    public async Task<IReadOnlyCollection<TaskListEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking()
        .OrderBy(x => x.Title)
        .Where(x => x.OwnerId == userId)
        .ToListAsync(cancellationToken);

    /// <summary>
    /// Retrieves a paged list of task lists owned by the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user who owns the task lists.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>
    /// A tuple containing the paged collection of task lists and the total number of items.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="page"/> or <paramref name="pageSize"/> is less than or equal to zero.
    /// </exception>
    public async Task<(IReadOnlyCollection<TaskListEntity> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        var query = this.DbSet.AsNoTracking().Where(tl => tl.OwnerId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(tl => tl.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

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
    public async Task<TaskListEntity?> GetByIdForUserAsync(Guid taskListId, Guid userId, CancellationToken cancellationToken = default)
        => await this.DbSet
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == taskListId && x.OwnerId == userId, cancellationToken);

    /// <summary>
    /// Determines whether the specified user is the owner of the given task list.
    /// </summary>
    /// <param name="taskListId">The identifier of the task list.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>
    /// <c>true</c> if the user is the owner of the task list; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> IsTaskListOwnerAsync(Guid taskListId, Guid userId, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking().AnyAsync(x => x.Id == taskListId && x.OwnerId == userId, cancellationToken);
}
