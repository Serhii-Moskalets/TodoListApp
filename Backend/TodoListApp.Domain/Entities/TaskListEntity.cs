using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a task list owned by a user, containing multiple tasks.
/// </summary>
[Table("Task_Lists")]
public class TaskListEntity : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskListEntity"/> class.
    /// </summary>
    /// <param name="ownerId">The ID of the user who created the task list.</param>
    /// <param name="title">The title of the task list.</param>
    public TaskListEntity(Guid ownerId, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title of the task list cannot be empty.", nameof(title));
        }

        this.OwnerId = ownerId;
        this.Title = title.Trim();
        this.CreatedDate = DateTime.UtcNow;
    }

    private TaskListEntity() { }

    /// <summary>
    /// Gets the title of the task list.
    /// </summary>
    [Column("Title")]
    required public string Title { get; init; }

    /// <summary>
    /// Gets the creation date of the task list.
    /// </summary>
    [Column("Created_Date")]
    required public DateTime CreatedDate { get; init; }

    /// <summary>
    /// Gets the ID of the user who owns this task list.
    /// </summary>
    [Column("Owner_Id")]
    required public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the user who owns this task list.
    /// </summary>
    public virtual UserEntity Owner { get; init; } = null!;

    /// <summary>
    /// Gets the collection of tasks contained in this task list.
    /// </summary>
    public ICollection<TaskEntity> Tasks { get; private set; } = new HashSet<TaskEntity>();
}
