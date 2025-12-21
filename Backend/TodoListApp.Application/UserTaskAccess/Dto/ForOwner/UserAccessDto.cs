namespace TodoListApp.Application.UserTaskAccess.Dto.ForOwner;

/// <summary>
/// Represents a user who has access to a task.
/// Contains basic user information.
/// </summary>
public class UserAccessDto
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the email of the user.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// Gets the full name of the user.
    /// </summary>
    public string UserName { get; init; } = null!;
}
