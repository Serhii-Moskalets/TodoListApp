namespace TodoListApp.Application.Task.Dtos;

/// <summary>
/// Data Transfer Object representing a tag.
/// </summary>
public class TagDto
{
    /// <summary>
    /// Gets the unique identifier of the tag.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    public string Name { get; init; } = null!;
}
