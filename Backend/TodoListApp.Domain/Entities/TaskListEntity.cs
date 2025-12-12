using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a task list owned by a user, containing multiple tasks.
/// </summary>
[Table("Task_Lists")]
public class TaskListEntity
{
    /// <summary>
    /// Gets the unique identifier of the task list.
    /// </summary>
    [Column("Task_List_Id")]
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the title of the task list.
    /// </summary>
    [Column("Title")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets the creation date of the task list.
    /// </summary>
    [Column("Created_Date")]
    public DateTime CreatedDate { get; private set;  } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the ID of the user who owns this task list.
    /// </summary>
    [Column("Owner_Id")]
    [ForeignKey(nameof(Owner))]
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the user who owns this task list.
    /// </summary>
    public UserEntity Owner { get; set; } = null!;

    /// <summary>
    /// Gets the collection of tasks contained in this task list.
    /// </summary>
    public List<TaskEntity> Tasks { get; private set; } = new();
}
