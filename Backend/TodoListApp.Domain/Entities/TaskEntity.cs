using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents a task within a task list, including its owner, status, due date, and related comments.
/// </summary>
[Table("Tasks")]
public class TaskEntity : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskEntity"/> class.
    /// </summary>
    /// <param name="ownerId">The ID of the user who created the task.</param>
    /// <param name="taskListId">The ID of the task list the task belongs to.</param>
    /// <param name="statusTask">The status task of the task.</param>
    /// <param name="title">The title of the task.</param>
    /// <param name="description">The Description of the task.</param>
    /// <param name="dueDate">The due date of the task.</param>
    public TaskEntity(
        Guid ownerId,
        Guid taskListId,
        StatusTask statusTask,
        string title,
        string? description = null,
        DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty", nameof(title));
        }

        this.OwnerId = ownerId;
        this.TaskListId = taskListId;
        this.Title = title.Trim();
        this.Description = description;
        this.CreatedDate = DateTime.UtcNow;
        this.DueDate = dueDate;
    }

    private TaskEntity() { }

    /// <summary>
    /// Gets the title of the task.
    /// </summary>
    [Column("Title")]
    required public string Title { get; init; }

    /// <summary>
    /// Gets the description of the task.
    /// </summary>
    [Column("Description")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets the time when the task was created.
    /// </summary>
    [Column("Created_Date")]
    required public DateTime CreatedDate { get; init; }

    /// <summary>
    /// Gets the due date of the task.
    /// </summary>
    [Column("Due_Date")]
    public DateTime? DueDate { get; init; }

    /// <summary>
    /// Gets the status of the task.
    /// </summary>
    [Column("Status")]
    required public StatusTask Status { get; init; }

    /// <summary>
    /// Gets the ID of the user who owns the task.
    /// </summary>
    [Column("Owner_Id")]
    required public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the ID of the task list that this task belongs to.
    /// </summary>
    [Column("Task_List_Id")]
    required public Guid TaskListId { get; init; }

    /// <summary>
    /// Gets the ID of the tag associated with the task, if any.
    /// </summary>
    [Column("Tag_Id")]
    public Guid? TagId { get; private set; }

    /// <summary>
    /// Gets the tag entity associated with the task.
    /// </summary>
    public virtual TagEntity? Tag { get; private set; }

    /// <summary>
    /// Gets the task list to which this task belongs.
    /// </summary>
    public virtual TaskListEntity TaskList { get; init; } = null!;

    /// <summary>
    /// Gets the owner of the task.
    /// </summary>
    public virtual UserEntity Owner { get; init; } = null!;

    /// <summary>
    /// Gets the collection of comments associated with this task.
    /// </summary>
    public virtual ICollection<CommentEntity> Comments { get; private set; } = new HashSet<CommentEntity>();

    /// <summary>
    /// Gets the collection of user accesses associated with this task.
    /// </summary>
    public virtual ICollection<UserTaskAccessEntity> UserAccesses { get; private set; } = new HashSet<UserTaskAccessEntity>();

    /// <summary>
    /// Removes the tag associated with the task by setting <see cref="Tag"/> and <see cref="TagId"/> to <c>null</c>.
    /// </summary>
    public void RemoveTag()
    {
        this.Tag = null;
        this.TagId = null;
    }
}
