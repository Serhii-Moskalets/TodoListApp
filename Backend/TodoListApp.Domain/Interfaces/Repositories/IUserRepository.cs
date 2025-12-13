using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for working with <see cref="UserEntity"/>.
/// Provides methods for querying, creating, updating, and deleting users.
/// </summary>
public interface IUserRepository : IRepository<UserEntity>
{
    /// <summary>
    /// Retrieves a user by their email.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// The <see cref="UserEntity"/> if a user with the specified email exists; otherwise, <c>null</c>.
    /// </returns>
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    /// <param name="userName">The username of the user.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// The <see cref="UserEntity"/> if a user with the specified username exists; otherwise, <c>null</c>.
    /// </returns>
    Task<UserEntity?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user with the specified email exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if a user with the specified email exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user with the specified username exists.
    /// </summary>
    /// <param name="userName">The username to check.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns><c>true</c> if a user with the specified username exists; otherwise, <c>false</c>.</returns>
    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for users by an optional search term.
    /// </summary>
    /// <param name="searchTerm">An optional term to filter users by username or email. If <c>null</c>, all users are returned.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A read-only collection of <see cref="UserEntity"/> that match the search criteria.</returns>
    Task<IReadOnlyCollection<UserEntity>> SearchAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
}
