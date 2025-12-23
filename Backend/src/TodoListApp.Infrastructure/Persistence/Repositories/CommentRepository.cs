using Microsoft.EntityFrameworkCore;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;

namespace TodoListApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="CommentEntity"/> objects in the database.
/// Provides methods to retrieve, check ownership, and paginate comments.
/// </summary>
public class CommentRepository(TodoListAppDbContext context)
    : BaseRepository<CommentEntity>(context), ICommentRepository
{
    /// <summary>
    /// Retrieves all comments associated with a specific task.
    /// </summary>
    /// <param name="taskId">The ID of the task for which to retrieve comments.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of comments for the specified task.</returns>
    public async Task<IReadOnlyCollection<CommentEntity>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
        => await this.DbSet
            .AsNoTracking()
            .Include(x => x.UserId)
            .Where(x => x.TaskId == taskId)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Retrieves a paged list of comments for a specific task.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of comments per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A tuple containing the paged comments and the total count of comments.</returns>
    public async Task<(IReadOnlyCollection<CommentEntity> Items, int TotalCount)> GetPagedCommentsByTaskIdAsync(Guid taskId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        var query = this.DbSet.AsNoTracking().Where(x => x.TaskId == taskId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(x => x.User)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Determines whether a specific user is the owner of a comment.
    /// </summary>
    /// <param name="commentId">The ID of the comment.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the user is the owner of the comment; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsCommentOwnerAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking().AnyAsync(x => x.Id == commentId && x.UserId == userId, cancellationToken);
}
