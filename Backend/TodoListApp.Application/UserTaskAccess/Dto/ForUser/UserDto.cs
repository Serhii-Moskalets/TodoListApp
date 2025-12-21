namespace TodoListApp.Application.UserTaskAccess.Dto.ForUser;

/// <summary>
/// Data Transfer Object representing a user.
/// Used to transfer user data between layers without exposing the entity directly.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    public string UserName { get; init; } = null!;
}
