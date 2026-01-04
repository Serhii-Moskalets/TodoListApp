using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Abstractions.Interfaces.Repositories;

/// <summary>
/// Repository interface for working with <see cref="CommentEntity"/>.
/// Provides methods for querying, creating, updating, and deleting comments.
/// </summary>
public interface ICommentRepository : IRepository<CommentEntity>
{
    /// <summary>
    /// Retrieves all comments associated with a specific task.
    /// </summary>
    /// <param name="taskId">The identifier of the task.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="CommentEntity"/>.</returns>
    Task<IReadOnlyCollection<CommentEntity>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated collection of comments for a specific task.
    /// </summary>
    /// <param name="taskId">The identifier of the task.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A tuple containing the paginated items and the total count of comments for the task.
    /// </returns>
    Task<(IReadOnlyCollection<CommentEntity> Items, int TotalCount)> GetPagedCommentsByTaskIdAsync(
        Guid taskId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
