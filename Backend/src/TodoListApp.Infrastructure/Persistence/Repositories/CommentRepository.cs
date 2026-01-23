using Microsoft.EntityFrameworkCore;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Extensions;
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
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of comments per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of comments for the specified task.</returns>
    public async Task<(IReadOnlyCollection<CommentEntity> Items, int TotalCount)> GetCommentsByTaskIdAsync(
        Guid taskId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var commentsQuery = this.DbSet.AsNoTracking()
            .Where(x => x.TaskId == taskId)
            .Include(x => x.User);

        var totalCount = await commentsQuery.CountAsync(cancellationToken);

        var items = await commentsQuery
            .OrderBy(x => x.CreatedDate)
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
