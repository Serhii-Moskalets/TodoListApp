namespace TodoListApp.Application.Abstractions;

/// <summary>
/// Provides the current date and time in UTC.
/// This abstraction allows for easier testing and decouples time retrieval from system calls.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current date and time in Coordinated Universal Time (UTC).
    /// </summary>
    DateTime UtcNow { get; }
}
