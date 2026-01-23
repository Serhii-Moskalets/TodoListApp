using Microsoft.EntityFrameworkCore;
using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="TaskListRepository"/> to verify its CRUD and query operations.
/// Includes checks for existence, retrieval, paging, sorting, and ownership verification.
/// </summary>
public class TaskListRepositoryTests
{
    /// <summary>
    /// Verifies that <see cref="TaskListRepository.ExistsByTitleAsync"/>
    /// returns true when a task list exists for the user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByTitle_ReturnTrue_WhenTaskListExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();
        var taskList = new TaskListEntity(userId, title: "Task List");
        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsByTitleAsync("Task List", userId);
        Assert.True(exists);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.ExistsByTitleAsync"/>
    /// returns false when a task list does not exist for the user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExistsByTitle_ReturnFalse_WhenTaskListDoesNotExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);
        var userId = Guid.NewGuid();

        var exists = await repo.ExistsByTitleAsync("Task List", userId);
        Assert.False(exists);
    }

    /// <summary>
    /// Ensures <see cref="TaskListRepository.ExistsByTitleAsync"/> throws <see cref="ArgumentException"/>
    /// when title is null, empty, or whitespace.
    /// </summary>
    /// <param name="title">The title to test for null, empty, or whitespace.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ExistsByTitle_MustThrow_WhenTitleIsNullOrWhiteSpace(string? title)
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(
            () => repo.ExistsByTitleAsync(title!, userId));
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetTaskListsAsync"/> returns a paged result
    /// with the correct items and total count.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetTaskListsAsync_ShouldReturnPagedResult_WithCorrectCount()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);
        var userId = Guid.NewGuid();

        var taskLists = new List<TaskListEntity>
        {
            new (userId, "List A"),
            new (userId, "List B"),
            new (userId, "List C"),
        };

        await context.AddRangeAsync(taskLists);
        await context.SaveChangesAsync();

        // Act - беремо 1 сторінку розміром 2
        var (items, totalCount) = await repo.GetTaskListsAsync(userId, page: 1, pageSize: 2);

        // Assert
        Assert.Equal(3, totalCount); // Всього 3
        Assert.Equal(2, items.Count); // На сторінці 2
        Assert.Contains(items, x => x.Title == "List A");
        Assert.Contains(items, x => x.Title == "List B");
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetTaskListsAsync"/> returns only
    /// lists belonging to the specified user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetTaskListsAsync_ShouldReturnOnlyUserSpecificLists()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        await context.AddRangeAsync(
            new TaskListEntity(user1, "User 1 List"),
            new TaskListEntity(user2, "User 2 List"));
        await context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await repo.GetTaskListsAsync(user1, 1, 10);

        // Assert
        Assert.Equal(1, totalCount);
        Assert.Single(items);
        Assert.All(items, x => Assert.Equal(user1, x.OwnerId));
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetTaskListByIdForUserAsync"/>
    /// retrieves the correct task list by its ID.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdForUser_ReturnTaskList_WhenTaskListExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();
        var taskList = new TaskListEntity(userId, title: "Task List");

        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        var saved = await repo.GetTaskListByIdForUserAsync(taskList.Id, userId);
        Assert.NotNull(saved);
        Assert.Equal(taskList.Title, saved.Title);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetTaskListByIdForUserAsync"/>
    /// returns null when the task list does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByIdForUser_ReturnTaskListEmpty_WhenTaskListDoesNotExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var saved = await repo.GetTaskListByIdForUserAsync(taskListId, userId);
        Assert.Null(saved);
    }

    /// <summary>
    /// Checks that <see cref="TaskListRepository.GetTaskListByIdForUserAsync"/>
    /// returns null when the task list does not belong to the specified user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTaskListByIdForUserAsync_ReturnsNull_WhenUserMismatch()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var taskList = new TaskListEntity(user1, title: "Task");
        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        var result = await repo.GetTaskListByIdForUserAsync(taskList.Id, user2);
        Assert.Null(result);
    }
}
