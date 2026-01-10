using System.ComponentModel.DataAnnotations.Schema;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;

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
    /// <param name="title">The title of the task.</param>
    /// <param name="dueDate">The due date of the task.</param>
    /// <param name="description">The Description of the task.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="title"/> is null, empty, or consists only of white-space characters or exceed 50 characters.
    /// Thrown when <paramref name="dueDate"/> in the past.
    /// Throw when <paramref name="description"/> exceed 1000 charecters.
    /// </exception>
    public TaskEntity(
        Guid ownerId,
        Guid taskListId,
        string title,
        DateTime? dueDate = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title cannot be empty.");
        }

        if (title.Length > 100)
        {
            throw new DomainException("Title cannot exceed 100 characters.");
        }

        if (dueDate < DateTime.UtcNow)
        {
            throw new DomainException("Due date cannot be in the past.");
        }

        if (description?.Length > 1000)
        {
            throw new DomainException("Description cannot exceed 1000 characters.");
        }

        this.OwnerId = ownerId;
        this.TaskListId = taskListId;
        this.Title = title.Trim();
        this.Description = description?.Trim();
        this.Status = StatusTask.NotStarted;
        this.CreatedDate = DateTime.UtcNow;
        this.DueDate = dueDate;
    }

    private TaskEntity() { }

    /// <summary>
    /// Gets the title of the task.
    /// </summary>
    [Column("Title")]
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Gets the description of the task.
    /// </summary>
    [Column("Description")]
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the time when the task was created.
    /// </summary>
    [Column("Created_Date")]
    public DateTime CreatedDate { get; init; }

    /// <summary>
    /// Gets the due date of the task.
    /// </summary>
    [Column("Due_Date")]
    public DateTime? DueDate { get; private set; }

    /// <summary>
    /// Gets the status of the task.
    /// </summary>
    [Column("Status")]
    public StatusTask Status { get; private set; }

    /// <summary>
    /// Gets the ID of the user who owns the task.
    /// </summary>
    [Column("Owner_Id")]
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the ID of the task list that this task belongs to.
    /// </summary>
    [Column("Task_List_Id")]
    public Guid TaskListId { get; init; }

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
    public virtual ICollection<CommentEntity> Comments { get; init; } = new HashSet<CommentEntity>();

    /// <summary>
    /// Gets the collection of user accesses associated with this task.
    /// </summary>
    public virtual ICollection<UserTaskAccessEntity> UserAccesses { get; init; } = new HashSet<UserTaskAccessEntity>();

    /// <summary>
    /// Updates the task title, description, and due date.
    /// </summary>
    /// <param name="title">The new title of the task.</param>
    /// <param name="description">The new description of the task.</param>
    /// <param name="dueDate">The new due date of the task.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="title"/> is null, empty, or consists only of white-space characters.
    /// Thrown when <paramref name="dueDate"/> is in the past.
    /// </exception>
    public void UpdateDetails(string title, string? description = null, DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title cannot be empty.");
        }

        if (title.Length > 100)
        {
            throw new DomainException("Title cannot exceed 100 characters.");
        }

        if (dueDate < DateTime.UtcNow)
        {
            throw new DomainException("Due date cannot be in the past.");
        }

        if (description?.Length > 1000)
        {
            throw new DomainException("Description cannot exceed 1000 characters.");
        }

        this.Title = title.Trim();
        this.Description = description?.Trim();
        this.DueDate = dueDate;
    }

    /// <summary>
    /// Sets or removes the tag associated with the task by ID.
    /// </summary>
    /// <param name="tagId">The ID of the tag to associate, or null to remove it.</param>
    public void SetTag(Guid? tagId)
    {
        if (this.TagId == tagId)
        {
            return;
        }

        this.TagId = tagId;
        if (tagId is null)
        {
            this.Tag = null;
        }
    }

    /// <summary>
    /// Changes the status of the task to the specified value.
    /// If the new status is the same as the current one, no action is taken.
    /// </summary>
    /// <param name="newStatus">The new status to apply to the task.</param>
    /// <exception cref="DomainException">
    /// Thrown when the provided <paramref name="newStatus"/> is not a valid <see cref="StatusTask"/> value.
    /// </exception>
    public void ChangeStatus(StatusTask newStatus)
    {
        if (this.Status == newStatus)
        {
            return;
        }

        switch (newStatus)
        {
            case StatusTask.NotStarted:
                this.SetNotStarted(); break;
            case StatusTask.InProgress:
                this.SetInProgress(); break;
            case StatusTask.Done:
                this.Complete(); break;
            default:
                throw new DomainException("Invalid task status.");
        }
    }

    /// <summary>
    /// Sets the task status to <see cref="StatusTask.NotStarted"/>.
    /// </summary>
    private void SetNotStarted()
    {
        this.Status = StatusTask.NotStarted;
    }

    /// <summary>
    /// Sets the task status to <see cref="StatusTask.InProgress"/>.
    /// </summary>
    private void SetInProgress()
    {
        if (this.Status == StatusTask.Done)
        {
            throw new DomainException("Cannot move from Done to InProgress.");
        }

        this.Status = StatusTask.InProgress;
    }

    /// <summary>
    /// Sets the task status to <see cref="StatusTask.Done"/>.
    /// </summary>
    private void Complete()
    {
        if (this.Status != StatusTask.InProgress)
        {
            throw new DomainException("Task must be InProgress to complete.");
        }

        this.Status = StatusTask.Done;
    }
}
