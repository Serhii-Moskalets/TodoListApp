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
    /// Checks that <see cref="TaskListRepository.GetByUserIdAsync"/>
    /// retrieves all task lists for a given user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserId_ReturnTaskLists_WhenTaskListExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();

        var taskLists = new[]
        {
            new TaskListEntity(userId, title: "Task List_1"),
            new TaskListEntity(userId, title: "Task List_2"),
            new TaskListEntity(userId, title: "Task List_3"),
            new TaskListEntity(userId, title: "Task List_4"),
        };

        foreach (var task in taskLists)
        {
            await repo.AddAsync(task);
        }

        await context.SaveChangesAsync();

        var saved = await repo.GetByUserIdAsync(userId);
        Assert.Equal(4, saved.Count);
    }

    /// <summary>
    /// Checks that <see cref="TaskListRepository.GetByUserIdAsync"/>
    /// returns task lists sorted by title.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserId_ShouldReturnTaskListsSortedByTitle()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();

        var taskLists = new[]
        {
            new TaskListEntity(userId, title: "Task List_4"),
            new TaskListEntity(userId, title: "Task List_1"),
            new TaskListEntity(userId, title: "Task List_3"),
            new TaskListEntity(userId, title: "Task List_2"),
        };

        foreach (var task in taskLists)
        {
            await repo.AddAsync(task);
        }

        await context.SaveChangesAsync();

        var saved = await repo.GetByUserIdAsync(userId);

        var expectdOrder = new[]
        {
            "Task List_1",
            "Task List_2",
            "Task List_3",
            "Task List_4",
        };

        var actualOrder = saved.Select(x => x.Title).ToArray();

        Assert.Equal(expectdOrder, actualOrder);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetByUserIdAsync"/>
    /// returns an empty list when the user has no task lists.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserId_ReturnTaskListEmpty_WhenUserHasNotTaskList()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);
        var userId = Guid.NewGuid();

        var saved = await repo.GetByUserIdAsync(userId);
        Assert.Empty(saved);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetByIdForUserAsync"/>
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

        var saved = await repo.GetByIdForUserAsync(taskList.Id, userId);
        Assert.NotNull(saved);
        Assert.Equal(taskList.Title, saved.Title);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetByIdForUserAsync"/>
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
        var saved = await repo.GetByIdForUserAsync(taskListId, userId);
        Assert.Null(saved);
    }

    /// <summary>
    /// Checks that <see cref="TaskListRepository.GetPagedByUserIdAsync"/>
    /// returns the correct page of task lists and total count.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPagedByUserId_ReturnCorrectPageAndTotalCount()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();

        var taskLists = new[]
        {
            new TaskListEntity(userId, title: "Task List_1"),
            new TaskListEntity(userId, title: "Task List_2"),
            new TaskListEntity(userId, title: "Task List_3"),
            new TaskListEntity(userId, title: "Task List_4"),
        };

        foreach (var task in taskLists)
        {
            await repo.AddAsync(task);
        }

        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetPagedByUserIdAsync(
            userId,
            page: 1,
            pageSize: 2);

        Assert.Equal(4, totalCount);
        Assert.Equal(2, items.Count);

        var titles = items.Select(x => x.Title).ToList();
        Assert.Equal(new[] { "Task List_1", "Task List_2" }, titles);
    }

    /// <summary>
    /// Checks that <see cref="TaskListRepository.GetPagedByUserIdAsync"/>
    /// returns the second page correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPagedByUserId_ReturnSecondPageCorrectrly()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();

        var taskLists = new[]
        {
            new TaskListEntity(userId, title: "Task List_1"),
            new TaskListEntity(userId, title: "Task List_2"),
            new TaskListEntity(userId, title: "Task List_3"),
            new TaskListEntity(userId, title: "Task List_4"),
        };

        foreach (var task in taskLists)
        {
            await repo.AddAsync(task);
        }

        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetPagedByUserIdAsync(
            userId,
            page: 2,
            pageSize: 2);

        Assert.Equal(4, totalCount);
        Assert.Equal(2, items.Count);

        var titles = items.Select(x => x.Title).ToList();
        Assert.Equal(new[] { "Task List_3", "Task List_4" }, titles);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.GetPagedByUserIdAsync"/>
    /// returns empty when the requested page is out of range.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPagedByUserId_ReturnEmpty_WhenPageIsOutOfRange()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();

        var taskLists = new[]
        {
            new TaskListEntity(userId, title: "Task List_1"),
            new TaskListEntity(userId, title: "Task List_2"),
        };

        foreach (var task in taskLists)
        {
            await repo.AddAsync(task);
        }

        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetPagedByUserIdAsync(
            userId,
            page: 3,
            pageSize: 2);

        Assert.Equal(2, totalCount);
        Assert.Empty(items);
    }

    /// <summary>
    /// Ensures <see cref="TaskListRepository.GetPagedByUserIdAsync"/>
    /// throws <see cref="ArgumentOutOfRangeException"/> when page number is invalid.
    /// </summary>
    /// <param name="page">The page number to test.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetPagedByUserId_MustThrow_WhenPageIsInvalid(int page)
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => repo.GetPagedByUserIdAsync(userId, page, pageSize: 10));
    }

    /// <summary>
    /// Ensures <see cref="TaskListRepository.GetPagedByUserIdAsync"/>
    /// throws <see cref="ArgumentOutOfRangeException"/> when page size is invalid.
    /// </summary>
    /// <param name="pageSize">The page size to test.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetPagedByUserId_MustThrow_WhenPageSizeIsInvalid(int pageSize)
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => repo.GetPagedByUserIdAsync(userId, page: 1, pageSize));
    }

    /// <summary>
    /// Checks that <see cref="TaskListRepository.GetPagedByUserIdAsync"/>
    /// returns items sorted by title.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPagedByUserId_ShouldReturnItemsSortedByTitle()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();

        var taskLists = new[]
        {
            new TaskListEntity(userId, title: "Task List_2"),
            new TaskListEntity(userId, title: "Task List_3"),
            new TaskListEntity(userId, title: "Task List_4"),
            new TaskListEntity(userId, title: "Task List_1"),
        };

        foreach (var task in taskLists)
        {
            await repo.AddAsync(task);
        }

        await context.SaveChangesAsync();

        var (items, _) = await repo.GetPagedByUserIdAsync(
            userId,
            page: 1,
            pageSize: 4);

        var expectdOrder = new[]
        {
            "Task List_1",
            "Task List_2",
            "Task List_3",
            "Task List_4",
        };

        var actualOrder = items.Select(x => x.Title).ToArray();

        Assert.Equal(expectdOrder, actualOrder);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.IsTaskListOwnerAsync"/>
    /// returns true when the user is the owner of the task list.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task IsTaskListOwner_ReturnTrue_WhenUserIsOwner()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId = Guid.NewGuid();
        var taskList = new TaskListEntity(userId, title: "Task List");

        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        var isOwner = await repo.IsTaskListOwnerAsync(taskList.Id, userId);

        Assert.True(isOwner);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListRepository.IsTaskListOwnerAsync"/>
    /// returns false when the user is not the owner of the task list.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task IsTaskListOwner_ReturnFalse_WhenUserIsNotOwner()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskListRepository(context);

        var userId_1 = Guid.NewGuid();
        var userId_2 = Guid.NewGuid();
        var taskList = new TaskListEntity(userId_1, title: "Task List");

        await repo.AddAsync(taskList);
        await context.SaveChangesAsync();

        var isOwner = await repo.IsTaskListOwnerAsync(taskList.Id, userId_2);

        Assert.False(isOwner);
    }
}
