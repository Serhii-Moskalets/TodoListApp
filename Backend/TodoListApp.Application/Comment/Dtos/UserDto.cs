namespace TodoListApp.Application.Comment.Dtos;

/// <summary>
/// Data Transfer Object (DTO) representing a user in the application layer.
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

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    public string Email { get; init; } = null!;
}
