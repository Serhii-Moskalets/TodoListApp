using Microsoft.EntityFrameworkCore;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="UserTaskAccessEntity"/> objects.
/// Provides methods to add, delete, check existence, and retrieve user-task access records,
/// including shared tasks and access checks for specific users and task lists.
/// </summary>
public class UserTaskAccessRepository : IUserTaskAccessRepository
{
    private readonly DbSet<UserTaskAccessEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTaskAccessRepository"/> class
    /// with the specified database context.
    /// </summary>
    /// <param name="context">The database context used for data operations.</param>
    public UserTaskAccessRepository(TodoListAppDbContext context)
    {
        this._dbSet = context.UserTaskAccesses;
    }

    /// <summary>
    /// Adds a new user-task access entry.
    /// </summary>
    /// <param name="entity">The <see cref="UserTaskAccessEntity"/> to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    public async Task AddAsync(UserTaskAccessEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await this._dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Deletes all user-task access entries for a specific task.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    public async Task DeleteAllByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        await this._dbSet
        .Where(x => x.TaskId == taskId)
        .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes all user-task access entries for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await this._dbSet
        .Where(x => x.UserId == userId)
        .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a specific user-task access entry.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    public async Task DeleteByTaskAndUserIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        await this._dbSet
        .Where(x => x.TaskId == taskId && x.UserId == userId)
        .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a user-task access entry by the user's email for a specific task.
    /// </summary>
    /// <param name="taskId">The ID of the task for which access should be removed.</param>
    /// <param name="email">The email of the user whose access should be deleted.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    public async Task DeleteByUserEmailAsync(Guid taskId, string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        await this._dbSet
            .Include(x => x.User)
            .Where(x => x.TaskId == taskId && x.User.Email == email)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Checks whether a user task access record exists for the given task and user.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="email">The user email.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if the access record exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsTaskAccessWithEmail(Guid taskId, string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return await this._dbSet.Include(x => x.User).AnyAsync(x => x.TaskId == taskId && x.User.Email == email, cancellationToken);
    }

    /// <summary>
    /// Checks if a user has access to a specific task.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that returns <c>true</c> if the user has access; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> ExistsAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
        => await this._dbSet.AnyAsync(x => x.TaskId == taskId && x.UserId == userId, cancellationToken);

    /// <summary>
    /// Retrieves a specific user-task access entry by task ID and user ID.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that returns the <see cref="UserTaskAccessEntity"/> if found; otherwise, <c>null</c>.
    /// </returns>
    public async Task<UserTaskAccessEntity?> GetByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
        => await this._dbSet.Include(x => x.Task).FirstOrDefaultAsync(x => x.TaskId == taskId && x.UserId == userId, cancellationToken);

    /// <summary>
    /// Retrieves all user-task access entries for a specific task, including user details.
    /// This is typically used by the owner of the task to see which users have access.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that returns a read-only collection of <see cref="UserTaskAccessEntity"/> entries
    /// representing all users who currently have access to the task.
    /// </returns>
    public async Task<IReadOnlyCollection<UserTaskAccessEntity>> GetTaskAccessesForOwnerTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
        => await this._dbSet
        .Include(x => x.User)
        .Where(x => x.TaskId == taskId)
        .ToListAsync(cancellationToken);

    /// <summary>
    /// Retrieves all user-task access entries for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// A task that returns a read-only collection of <see cref="UserTaskAccessEntity"/>
    /// entries representing the tasks shared with the user.
    /// </returns>
    public async Task<IReadOnlyCollection<UserTaskAccessEntity>> GetSharedTasksForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await this._dbSet
        .Include(x => x.Task)
        .Where(x => x.UserId == userId)
        .ToListAsync(cancellationToken);
}
