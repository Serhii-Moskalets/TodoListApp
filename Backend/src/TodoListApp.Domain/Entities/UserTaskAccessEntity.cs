using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents the access relationship between a user and a task.
/// </summary>
[Table("User_Task_Access")]
public class UserTaskAccessEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserTaskAccessEntity"/> class.
    /// </summary>
    /// <param name="taskId">The ID of the task.</param>
    /// <param name="userId">The ID of the user.</param>
    public UserTaskAccessEntity(Guid taskId, Guid userId)
    {
        this.TaskId = taskId;
        this.UserId = userId;
    }

    private UserTaskAccessEntity() { }

    /// <summary>
    /// Gets the ID of the task.
    /// </summary>
    [Column("Task_Id")]
    public Guid TaskId { get; init; }

    /// <summary>
    /// Gets the ID of the user.
    /// </summary>
    [Column("User_Id")]
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the task associated with this access.
    /// </summary>
    public virtual TaskEntity Task { get; init; } = null!;

    /// <summary>
    /// Gets the user associated with this access.
    /// </summary>
    public virtual UserEntity User { get; init; } = null!;
}
