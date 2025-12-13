using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for working with <see cref="CommentEntity"/>.
/// Provides methods for querying, creating, updating, and deleting comments.
/// </summary>
public interface ICommentRepository
{
    /// <summary>
    /// Retrieves a comment by its identifier.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// The <see cref="CommentEntity"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<CommentEntity?> GetByIdAsync(Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all comments associated with a specific task.
    /// </summary>
    /// <param name="taskId">The identifier of the task.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="CommentEntity"/>.</returns>
    Task<IReadOnlyCollection<CommentEntity>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all comments created by a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="CommentEntity"/>.</returns>
    Task<IReadOnlyCollection<CommentEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Checks whether a comment exists by its identifier.
    /// </summary>
    /// <param name="commentId">The identifier of the comment.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the comment exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsAsync(Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a given user is the owner of a specific comment.
    /// </summary>
    /// <param name="comment">The identifier of the comment.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the user is the owner of the comment; otherwise, <c>false</c>.</returns>
    Task<bool> IsCommentOwnerAsync(Guid comment, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new comment to the repository.
    /// </summary>
    /// <param name="comment">The <see cref="CommentEntity"/> to add.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(CommentEntity comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing comment in the repository.
    /// </summary>
    /// <param name="comment">The <see cref="CommentEntity"/> to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task UpdateAsync(CommentEntity comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a comment by its identifier.
    /// </summary>
    /// <param name="commentId">The identifier of the comment to delete.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    Task Delete(Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
    /// The result contains the number of state entries written to the database.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
