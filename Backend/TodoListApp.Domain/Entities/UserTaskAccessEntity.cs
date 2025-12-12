using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents the access relationship between a user and a task.
/// </summary>
[Table("User_Task_Access")]
public class UserTaskAccessEntity
{
    /// <summary>
    /// Gets or sets the ID of the task.
    /// </summary>
    [Column("Task_Id")]
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user.
    /// </summary>
    [Column("User_Id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the task associated with this access.
    /// </summary>
    public TaskEntity Task { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user associated with this access.
    /// </summary>
    public UserEntity User { get; set; } = null!;
}
