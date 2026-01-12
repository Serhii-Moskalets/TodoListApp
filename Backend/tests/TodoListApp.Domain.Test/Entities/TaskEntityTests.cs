using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="TaskEntity"/> domain entity.
/// </summary>
public class TaskEntityTests
{
    private const string Title = "Task title";
    private const string Description = "Description description";

    /// <summary>
    /// Verifies that the constructor creates a task
    /// when all valid parameters are provided.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTask_WhenValidData()
    {
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddMinutes(5);

        var task = new TaskEntity(ownerId, taskListId, Title, dueDate, Description);

        Assert.Equal(ownerId, task.OwnerId);
        Assert.Equal(taskListId, task.TaskListId);
        Assert.Equal(Title, task.Title);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(Description, task.Description);
        Assert.Equal(StatusTask.NotStarted, task.Status);
        Assert.True(task.CreatedDate <= DateTime.UtcNow);
    }

    /// <summary>
    /// Verifies that a task can be created without a due date.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTask_WithoutDueDate_WhenValidData()
    {
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        var task = new TaskEntity(ownerId, taskListId, Title, description: Description);

        Assert.Equal(ownerId, task.OwnerId);
        Assert.Equal(taskListId, task.TaskListId);
        Assert.Equal(Title, task.Title);
        Assert.Null(task.DueDate);
        Assert.Equal(Description, task.Description);
        Assert.True((DateTime.UtcNow - task.CreatedDate).TotalSeconds < 1);
    }

    /// <summary>
    /// Verifies that a task can be created without a description.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTask_WithoutDescription_WhenValidData()
    {
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddMinutes(5);

        var task = new TaskEntity(ownerId, taskListId, Title, dueDate: dueDate);

        Assert.Equal(ownerId, task.OwnerId);
        Assert.Equal(taskListId, task.TaskListId);
        Assert.Equal(Title, task.Title);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Null(task.Description);
        Assert.True((DateTime.UtcNow - task.CreatedDate).TotalSeconds < 1);
    }

    /// <summary>
    /// Verifies that a task can be created without a due date and description.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTask_WithoutDueDateAndDescription_WhenValidData()
    {
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        var task = new TaskEntity(ownerId, taskListId, Title);

        Assert.Equal(ownerId, task.OwnerId);
        Assert.Equal(taskListId, task.TaskListId);
        Assert.Equal(Title, task.Title);
        Assert.Null(task.DueDate);
        Assert.Null(task.Description);
        Assert.True((DateTime.UtcNow - task.CreatedDate).TotalSeconds < 1);
    }

    /// <summary>
    /// Verifies that the constructor throws an <see cref="DomainException"/>
    /// when the task title is invalid.
    /// </summary>
    /// <param name="invalidTitle">An invalid task title.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenTitleInvalid(string? invalidTitle)
    {
        Assert.Throws<DomainException>(() =>
        new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            invalidTitle!));
    }

    /// <summary>
    /// Verifies that the constructor trims whitespace from the task title.
    /// </summary>
    /// <param name="title">A title containing leading or trailing whitespace.</param>
    [Theory]
    [InlineData("   Task title   ")]
    [InlineData("Task title   ")]
    [InlineData("   Task title")]
    public void Constructor_ShouldTrimName(string title)
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            title);

        Assert.Equal(Title, task.Title);
    }

    /// <summary>
    /// Verifies that the constructor trims whitespace from the task description.
    /// </summary>
    /// <param name="description">A description containing leading or trailing whitespace.</param>
    [Theory]
    [InlineData("   Description description   ")]
    [InlineData("Description description   ")]
    [InlineData("   Description description")]
    public void Constructor_ShouldTrimDescription(string description)
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Title,
            description: description);

        Assert.Equal(Description, task.Description);
    }

    /// <summary>
    /// Verifies that the comments collection is initialized on creation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeCommentsCollection()
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Title);
        Assert.NotNull(task.Comments);
        Assert.Empty(task.Comments);
    }

    /// <summary>
    /// Verifies that the user accesses collection is initialized on creation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeUserAccessesCollection()
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Title);
        Assert.NotNull(task.UserAccesses);
        Assert.Empty(task.UserAccesses);
    }

    /// <summary>
    /// Verifies that task details are updated when only a new title is provided.
    /// </summary>
    [Fact]
    public void Update_ShouldChangeDetails_WhenValid_WithoutDueDateAndDescription()
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Old title");

        task.UpdateDetails("New title");

        Assert.Equal("New title", task.Title);
    }

    /// <summary>
    /// Verifies that updating task details throws an <see cref="DomainException"/>
    /// when the title is invalid.
    /// </summary>
    /// <param name="invalidTitle">An invalid task title.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_ShouldThrow_WhenTitleInvalid(string? invalidTitle)
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Old title");

        Assert.Throws<DomainException>(() => task.UpdateDetails(invalidTitle!));
    }

    /// <summary>
    /// Verifies that task details are updated when valid title, description,
    /// and due date are provided.
    /// </summary>
    [Fact]
    public void Update_ShouldChangeDetails_WhenValidWithDueDateAndDescription()
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Old title");
        var newDueDate = DateTime.UtcNow.AddMinutes(5);

        task.UpdateDetails("New title", Description, newDueDate);

        Assert.Equal("New title", task.Title);
        Assert.Equal(Description, task.Description);
        Assert.Equal(newDueDate, task.DueDate);
        Assert.Equal(newDueDate, task.DueDate);
    }

    /// <summary>
    /// Verifies that <see cref="TaskEntity.UpdateDetails"/> trims whitespace
    /// from the updated title.
    /// </summary>
    /// <param name="title">A title containing leading or trailing whitespace.</param>
    [Theory]
    [InlineData("   New title   ")]
    [InlineData("New title   ")]
    [InlineData("   New title")]
    public void UpdateDetails_ShouldTrimTitle(string title)
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Old title");
        task.UpdateDetails(title);
        Assert.Equal("New title", task.Title);
    }

    /// <summary>
    /// Verifies that <see cref="TaskEntity.UpdateDetails"/> trims whitespace
    /// from the updated description.
    /// </summary>
    /// <param name="description">A description containing leading or trailing whitespace.</param>
    [Theory]
    [InlineData("   Description description  ")]
    [InlineData("Description description ")]
    [InlineData("   Description description")]
    public void UpdateDetails_ShouldTrimDescription(string description)
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);
        task.UpdateDetails(Title, description);
        Assert.Equal(Description, task.Description);
    }

    /// <summary>
    /// Verifies that a task can transition from InProgress to Done status.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldCompleteTask_WhenInProgress()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);
        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.Done);
        Assert.Equal(StatusTask.Done, task.Status);
    }

    /// <summary>
    /// Verifies that changing task status from Done back to InProgress
    /// throws a <see cref="DomainException"/>.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldThrow_WhenDoneToInProgress()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);
        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.Done);

        Assert.Throws<DomainException>(() => task.ChangeStatus(StatusTask.InProgress));
    }

    /// <summary>
    /// Verifies that changing task status to the same value
    /// does not modify the task state.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldDoNothing_WhenSameStatus()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);

        task.ChangeStatus(StatusTask.NotStarted);
        Assert.Equal(StatusTask.NotStarted, task.Status);

        task.ChangeStatus(StatusTask.NotStarted);
        Assert.Equal(StatusTask.NotStarted, task.Status);

        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.InProgress);
        Assert.Equal(StatusTask.InProgress, task.Status);
    }

    /// <summary>
    /// Verifies that providing an invalid status enum
    /// throws a <see cref="DomainException"/>.
    /// </summary>
    [Fact]
    public void ChangeStatus_ShouldThrow_WhenInvalidEnum()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);
        const StatusTask invalidStatus = (StatusTask)999;

        Assert.Throws<DomainException>(() => task.ChangeStatus(invalidStatus));
    }

    /// <summary>
    /// Verifies that setting a tag assigns the tag ID
    /// and resets the navigation property.
    /// </summary>
    [Fact]
    public void SetTag_ShouldUpdateTagIdAndResetTag()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);
        var tagId = Guid.NewGuid();

        task.SetTag(tagId);

        Assert.Equal(tagId, task.TagId);
        Assert.Null(task.Tag);
    }

    /// <summary>
    /// Verifies that setting a null tag removes the tag
    /// from the task.
    /// </summary>
    [Fact]
    public void SetTag_ShouldRemoveTag_WhenNull()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), Title);
        var tagId = Guid.NewGuid();
        task.SetTag(tagId);

        task.SetTag(null);

        Assert.Null(task.TagId);
        Assert.Null(task.Tag);
    }
}
