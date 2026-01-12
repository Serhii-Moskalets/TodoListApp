using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="TaskListEntity"/> domain entity.
/// </summary>
public class TaskListEntityTest
{
    private const string Title = "Task list title";

    /// <summary>
    /// Verifies that the constructor creates a task list
    /// when valid owner ID and title are provided.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTaskList_WhenValidData()
    {
        var ownerId = Guid.NewGuid();

        var taskList = new TaskListEntity(ownerId, Title);

        Assert.Equal(ownerId, taskList.OwnerId);
        Assert.Equal(Title, taskList.Title);
        Assert.True(DateTime.UtcNow >= taskList.CreatedDate);
    }

    /// <summary>
    /// Verifies that the constructor throws an <see cref="DomainException"/>
    /// when the title is null, empty, or whitespace.
    /// </summary>
    /// <param name="invalidTitle">An invalid task list title.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenTitleInvalid(string? invalidTitle)
    {
        Assert.Throws<DomainException>(() =>
        new TaskListEntity(
            Guid.NewGuid(),
            invalidTitle!));
    }

    /// <summary>
    /// Verifies that the constructor trims whitespace
    /// from the task list title.
    /// </summary>
    /// <param name="title">A title containing leading or trailing whitespace.</param>
    [Theory]
    [InlineData("   Task list title   ")]
    [InlineData("Task list title   ")]
    [InlineData("   Task list title")]
    public void Constructor_ShouldTrimTitle(string title)
    {
        var taskList = new TaskListEntity(
            Guid.NewGuid(),
            title);

        Assert.Equal(Title, taskList.Title);
    }

    /// <summary>
    /// Verifies that the tasks collection is initialized
    /// when a task list is created.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeTasksCollection()
    {
        var taskList = new TaskListEntity(
            Guid.NewGuid(),
            Title);
        Assert.NotNull(taskList.Tasks);
        Assert.Empty(taskList.Tasks);
    }

    /// <summary>
    /// Verifies that the task list title is updated
    /// when a valid new title is provided.
    /// </summary>
    [Fact]
    public void Update_ShouldChangeTitle()
    {
        var taskList = new TaskListEntity(
            Guid.NewGuid(),
            Title);

        taskList.UpdateTitle("New title");

        Assert.Equal("New title", taskList.Title);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListEntity.UpdateTitle"/>
    /// throws an <see cref="DomainException"/>
    /// when the new title is invalid.
    /// </summary>
    /// <param name="invalidTitle">An invalid new title.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_ShouldThrow_WhenTitleInvalid(string? invalidTitle)
    {
        var taskList = new TaskListEntity(
            Guid.NewGuid(),
            "Old title");

        Assert.Throws<DomainException>(() => taskList.UpdateTitle(invalidTitle!));
    }

    /// <summary>
    /// Verifies that <see cref="TaskListEntity.UpdateTitle"/>
    /// trims whitespace from the new title.
    /// </summary>
    /// <param name="title">A new title containing leading or trailing whitespace.</param>
    [Theory]
    [InlineData("   New title   ")]
    [InlineData("New title   ")]
    [InlineData("   New title")]
    public void Update_ShouldTrimTitle(string title)
    {
        var taskList = new TaskListEntity(
            Guid.NewGuid(),
            "Old title");

        taskList.UpdateTitle(title);
        Assert.Equal("New title", taskList.Title);
    }
}
