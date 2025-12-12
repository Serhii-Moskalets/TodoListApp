using System.ComponentModel.DataAnnotations.Schema;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a task within a task list, including its owner, status, due date, and related comments.
/// </summary>
[Table("Tasks")]
public class TaskEntity
{
    /// <summary>
    /// Gets the unique identifier of the task.
    /// </summary>
    [Column("Task_Id")]
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the title of the task.
    /// </summary>
    [Column("Title")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the task.
    /// </summary>
    [Column("Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets the date and time when the task was created.
    /// </summary>
    [Column("Created_Date")]
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the due date of the task.
    /// </summary>
    [Column("Due_Date")]
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the status of the task.
    /// </summary>
    [Column("Status")]
    public StatusTask Status { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who owns the task.
    /// </summary>
    [Column("Owner_Id")]
    [ForeignKey(nameof(Owner))]
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the task list that this task belongs to.
    /// </summary>
    [Column("Task_List_Id")]
    [ForeignKey(nameof(TaskList))]
    public Guid TaskListId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the tag associated with the task, if any.
    /// </summary>
    [Column("Tag_Id")]
    [ForeignKey(nameof(Tag))]
    public Guid? TagId { get; set; }

    /// <summary>
    /// Gets or sets the tag entity associated with the task.
    /// </summary>
    public TagEntity? Tag { get; set; }

    /// <summary>
    /// Gets or sets the task list to which this task belongs.
    /// </summary>
    public TaskListEntity TaskList { get; set; } = null!;

    /// <summary>
    /// Gets or sets the owner of the task.
    /// </summary>
    public UserEntity Owner { get; set; } = null!;

    /// <summary>
    /// Gets the collection of comments associated with this task.
    /// </summary>
    public List<CommentEntity> Comments { get; private set; } = new();

    /// <summary>
    /// Gets the collection of user accesses associated with this task.
    /// </summary>
    public List<UserTaskAccessEntity> UserAccesses { get; private set; } = new();
}
