using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Test.Entities;

public class TaskEntityTests
{
    [Fact]
    public void Constructor_ShouldCreateTask_WhenValidData()
    {
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var title = "Task title";
        var dueDate = DateTime.UtcNow;
        var description = "Description";

        var task = new TaskEntity(ownerId, taskListId, title, dueDate, description);

        Assert.Equal(ownerId, task.OwnerId);
        Assert.Equal(taskListId, task.TaskListId);
        Assert.Equal(title, task.Title);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(description, task.Description);
        Assert.Equal(StatusTask.NotStarted, task.Status);
        Assert.True(task.CreatedDate <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_ShouldCreateTask_WithoutDueDate_WhenValidData()
    {
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var title = "Task title";
        var description = "Description";

        var task = new TaskEntity(ownerId, taskListId, title, description: description);

        Assert.Equal(ownerId, task.OwnerId);
        Assert.Equal(taskListId, task.TaskListId);
        Assert.Equal(title, task.Title);
        Assert.Null(task.DueDate);
        Assert.Equal(description, task.Description);
        Assert.True((DateTime.UtcNow - task.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void Constructor_ShouldCreateTask_WithoutDescription_WhenValidData()
    {
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var title = "Task title";
        var dueDate = DateTime.UtcNow;

        var task = new TaskEntity(ownerId, taskListId, title, dueDate: dueDate);

        Assert.Equal(ownerId, task.OwnerId);
        Assert.Equal(taskListId, task.TaskListId);
        Assert.Equal(title, task.Title);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Null(task.Description);
        Assert.True((DateTime.UtcNow - task.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void Constructor_ShouldCreateTask_WithoutDueDateAndDescription_WhenValidData()
    {
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var title = "Task title";

        var task = new TaskEntity(ownerId, taskListId, title);

        Assert.Equal(ownerId, task.OwnerId);
        Assert.Equal(taskListId, task.TaskListId);
        Assert.Equal(title, task.Title);
        Assert.Null(task.DueDate);
        Assert.Null(task.Description);
        Assert.True((DateTime.UtcNow - task.CreatedDate).TotalSeconds < 1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenTitleInvalid(string? invalidTitle)
    {
        Assert.Throws<ArgumentException>(() => 
        new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            invalidTitle!));
    }

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

        Assert.Equal("Task title", task.Title);
    }

    [Theory]
    [InlineData("   Description Description   ")]
    [InlineData("Description Description   ")]
    [InlineData("   Description Description")]
    public void Constructor_ShouldTrimDescription(string description)
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title", 
            description: description);

        Assert.Equal("Description Description", task.Description);
    }

    [Fact]
    public void Constructor_ShouldInitializeCommentsCollection()
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title");
        Assert.NotNull(task.Comments);
        Assert.Empty(task.Comments);
    }

    [Fact]
    public void Constructor_ShouldInitializeUserAccessesCollection()
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title");
        Assert.NotNull(task.UserAccesses);
        Assert.Empty(task.UserAccesses);
    }

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

        Assert.Throws<ArgumentException>(() => task.UpdateDetails(invalidTitle!));
    }

    [Fact]
    public void Update_ShouldChangeDetails_WhenValidWithDueDateAndDescription()
    {
        var task = new TaskEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Old title");
        var newDueDate = DateTime.UtcNow;

        task.UpdateDetails("New title", "description", newDueDate);

        Assert.Equal("New title", task.Title);
        Assert.Equal("description", task.Description);
        Assert.Equal(newDueDate, task.DueDate);
        Assert.Equal(newDueDate, task.DueDate);
    }

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

    [Theory]
    [InlineData("   Description   ")]
    [InlineData("Description   ")]
    [InlineData("   Description")]
    public void UpdateDetails_ShouldTrimDescription(string description)
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Title");
        task.UpdateDetails("Title", description);
        Assert.Equal("Description", task.Description);
    }

    [Fact]
    public void ChangeStatus_ShouldCompleteTask_WhenInProgress()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Title");
        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.Done);
        Assert.Equal(StatusTask.Done, task.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldThrow_WhenDoneToInProgress()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Title");
        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.Done);

        Assert.Throws<DomainException>(() => task.ChangeStatus(StatusTask.InProgress));
    }

    [Fact]
    public void ChangeStatus_ShouldDoNothing_WhenSameStatus()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Title");

        task.ChangeStatus(StatusTask.NotStarted);
        Assert.Equal(StatusTask.NotStarted, task.Status);

        task.ChangeStatus(StatusTask.NotStarted); // нічого не змінюється
        Assert.Equal(StatusTask.NotStarted, task.Status);

        task.ChangeStatus(StatusTask.InProgress);
        task.ChangeStatus(StatusTask.InProgress); // нічого не змінюється
        Assert.Equal(StatusTask.InProgress, task.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldThrow_WhenInvalidEnum()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Title");
        var invalidStatus = (StatusTask)999;

        Assert.Throws<DomainException>(() => task.ChangeStatus(invalidStatus));
    }

    [Fact]
    public void SetTag_ShouldUpdateTagIdAndResetTag()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Title");
        var tagId = Guid.NewGuid();

        task.SetTag(tagId);

        Assert.Equal(tagId, task.TagId);
        Assert.Null(task.Tag);
    }

    [Fact]
    public void SetTag_ShouldRemoveTag_WhenNull()
    {
        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Title");
        var tagId = Guid.NewGuid();
        task.SetTag(tagId);

        task.SetTag(null);

        Assert.Null(task.TagId);
        Assert.Null(task.Tag);
    }
}
