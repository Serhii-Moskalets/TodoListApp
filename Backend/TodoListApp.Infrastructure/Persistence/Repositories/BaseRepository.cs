using Microsoft.EntityFrameworkCore;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository providing basic CRUD operations for any entity type.
/// This class implements <see cref="IRepository{TEntity}"/> and serves as a base
/// for concrete repositories that manage specific entity types.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be managed. Must be a class.</typeparam>
public class BaseRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private readonly TodoListAppDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{TEntity}"/> class
    /// with the specified database context.
    /// </summary>
    /// <param name="context">The database context used for data operations.</param>
    public BaseRepository(TodoListAppDbContext context)
    {
        this._context = context;
        this._dbSet = this._context.Set<TEntity>();
    }

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await this._dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Deletes an entity from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await this.GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            this._dbSet.Remove(entity);
        }
    }

    /// <summary>
    /// Checks whether an entity exists in the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that returns <c>true</c> if the entity exists; otherwise, <c>false</c>.</returns>
    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await this.GetByIdAsync(id, cancellationToken);
        return entity != null;
    }

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that returns the entity if found; otherwise, <c>null</c>.</returns>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this._dbSet.FindAsync([id], cancellationToken);
    }

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        this._dbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Persists all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that returns the number of state entries written to the database.</returns>
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await this._context.SaveChangesAsync(cancellationToken);
    }
}
