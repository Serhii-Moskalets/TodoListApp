using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for working with <see cref="TagEntity"/>.
/// Provides methods for querying, creating, updating, and deleting tags.
/// </summary>
public interface ITagRepository : IRepository<TagEntity>
{
    /// <summary>
    /// Retrieves all tags owned by a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="TagEntity"/>.</returns>
    Task<IReadOnlyCollection<TagEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated collection of tags for a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A tuple containing the paginated items and the total count of tags.</returns>
    Task<(IReadOnlyCollection<TagEntity> Items, int TotalCount)> GetPagedTagsByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a tag with the specified name exists for a specific user.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if a tag with the specified name exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByNameAsync(string name, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a specific user is the owner of a tag.
    /// </summary>
    /// <param name="tagId">The identifier of the tag.</param>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the user is the owner of the tag; otherwise, <c>false</c>.</returns>
    Task<bool> IsTagOwnerAsync(Guid tagId, Guid userId, CancellationToken cancellationToken = default);
}
