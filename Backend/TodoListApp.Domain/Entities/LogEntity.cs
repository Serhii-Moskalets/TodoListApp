using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a log entry in the system, used for tracking application events and errors.
/// </summary>
[Table("Logs")]
public class LogEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the log entry.
    /// </summary>
    [Key]
    public int LogId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the log entry was created.
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the severity level of the log entry (e.g., Information, Warning, Error).
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Level { get; set; } = null!;

    /// <summary>
    /// Gets or sets the message of the log entry.
    /// </summary>
    [Required]
    public string Message { get; set; } = null!;

    /// <summary>
    /// Gets or sets the exception details associated with the log entry, if any.
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Gets or sets additional context or metadata for the log entry.
    /// </summary>
    public string? Context { get; set; }
}
