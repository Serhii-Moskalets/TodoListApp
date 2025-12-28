using Microsoft.EntityFrameworkCore;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;

namespace TodoListApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="TagEntity"/> objects in the database.
/// Provides methods to check existence, retrieve, paginate, and verify ownership of tags.
/// Initializes a new instance of the repository with the specified database context.
/// </summary>
public class TagRepository(TodoListAppDbContext context)
    : BaseRepository<TagEntity>(context), ITagRepository
{
    /// <summary>
    /// Checks if a tag with the specified name exists for a specific user.
    /// </summary>
    /// <param name="name">The name of the tag to check.</param>
    /// <param name="userId">The ID of the user who owns the tag.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if a tag with the specified name exists for the user; otherwise, <c>false</c>.</returns>
    public Task<bool> ExistsByNameAsync(string name, Guid userId, CancellationToken cancellationToken = default)
        => this.DbSet.AsNoTracking().AnyAsync(x => x.UserId == userId && x.Name == name, cancellationToken);

    /// <summary>
    /// Retrieves all tags associated with a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of tags for the specified user.</returns>
    public async Task<IReadOnlyCollection<TagEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking().Where(x => x.UserId == userId).OrderBy(t => t.Name).ToListAsync(cancellationToken);

    /// <summary>
    /// Retrieves a paged list of tags for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of tags per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A tuple containing the paged tags and the total count of tags.</returns>
    public async Task<(IReadOnlyCollection<TagEntity> Items, int TotalCount)> GetPagedTagsByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        var query = this.DbSet.AsNoTracking().Where(x => x.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Determines whether a specific user is the owner of a tag.
    /// </summary>
    /// <param name="tagId">The ID of the tag.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the user is the owner of the tag; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsTagOwnerAsync(Guid tagId, Guid userId, CancellationToken cancellationToken = default)
        => await this.DbSet.AsNoTracking().AnyAsync(x => x.Id == tagId && x.UserId == userId, cancellationToken);
}
