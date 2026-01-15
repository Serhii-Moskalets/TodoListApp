using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="TaskRepository"/>.
/// Tests cover existence checks, overdue task handling, pagination, filtering, and ownership verification.
/// </summary>
public class TaskRepositoryTests
{
    private readonly TodoListAppDbContext _context;
    private readonly TaskRepository _repo;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskRepositoryTests"/> class.
    /// </summary>
    public TaskRepositoryTests()
    {
        this._context = InMemoryDbContextFactory.Create();
        this._repo = new TaskRepository(this._context);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.CountOverdueTasksAsync"/>
    /// returns the correct number of overdue tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CountOverdueTask_ReturnsCorrectCount()
    {
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        var pastTime = DateTime.UtcNow.AddMinutes(5);
        var now = DateTime.UtcNow.AddMinutes(20);

        var tasks = new[]
        {
            new TaskEntity(userId, taskListId, "Task_1", pastTime),
            new TaskEntity(userId, taskListId, "Task_2", pastTime),
            new TaskEntity(userId, taskListId, "Task_3", now.AddMinutes(10)),
        };

        foreach (var task in tasks)
        {
            await this._repo.AddAsync(task);
        }

        await this._context.SaveChangesAsync();

        var count = await this._repo.CountOverdueTasksAsync(userId, taskListId, now);
        Assert.Equal(2, count);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.IsTaskOwnerAsync"/>
    /// returns correct ownership status for a task.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task IsOwnerTask_ReturnsCorrectValue()
    {
        var userId_1 = Guid.NewGuid();
        var userId_2 = Guid.NewGuid();
        var task = new TaskEntity(userId_1, Guid.NewGuid(), "Task");
        await this._repo.AddAsync(task);
        await this._context.SaveChangesAsync();

        Assert.True(await this._repo.IsTaskOwnerAsync(task.Id, userId_1));
        Assert.False(await this._repo.IsTaskOwnerAsync(task.Id, userId_2));
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.GetTasksAsync"/> correctly applies pagination parameters
    /// and returns the expected subset of tasks along with the total count.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetTasksAsync_ReturnsCorrectPageAndTotalCount()
    {
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        for (int i = 1; i <= 5; i++)
        {
            var task = new TaskEntity(userId, taskListId, $"Task_{i}", DateTime.UtcNow.AddDays(i));
            await this._repo.AddAsync(task);
        }

        await this._context.SaveChangesAsync();

        var (items, total) = await this._repo.GetTasksAsync(
            userId,
            taskListId,
            page: 2,
            pageSize: 2);

        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.SearchByTitleAsync"/> filters tasks by a title substring
    /// and respects pagination limits.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task SearchByTitleAsync_ReturnsPaginatedResults()
    {
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var tasks = new[]
        {
            new TaskEntity(userId, listId, "Apple"),
            new TaskEntity(userId, listId, "Application"),
            new TaskEntity(userId, listId, "Banana"),
        };

        foreach (var task in tasks)
        {
            await this._repo.AddAsync(task);
        }

        await this._context.SaveChangesAsync();

        var (items, total) = await this._repo.SearchByTitleAsync(userId, "App", page: 1, pageSize: 10);

        Assert.Equal(2, total);
        Assert.All(items, x => Assert.Contains("App", x.Title));
    }

    /// <summary>
    /// Verifies that <see cref="TaskRepository.GetTasksAsync"/> ignores status filtering
    /// when the status collection is null or empty.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetTasks_ReturnsAll_WhenStatusesNullOrEmpty()
    {
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        await this._repo.AddAsync(new TaskEntity(userId, taskListId, "Task_1"));
        await this._repo.AddAsync(new TaskEntity(userId, taskListId, "Task_2"));
        await this._context.SaveChangesAsync();

        var (_, total1) = await this._repo.GetTasksAsync(userId, taskListId, statuses: null);
        var (items2, _) = await this._repo.GetTasksAsync(userId, taskListId, statuses: []);

        Assert.Equal(2, total1);
        Assert.Equal(2, items2.Count);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.GetTaskByIdForUserAsync"/> returns null
    /// when a user attempts to retrieve a task that belongs to a different user.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetTaskByIdForUser_ReturnsNull_WhenUserMismatch()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var task = new TaskEntity(user1, Guid.NewGuid(), "Task");
        await this._repo.AddAsync(task);
        await this._context.SaveChangesAsync();

        var result = await this._repo.GetTaskByIdForUserAsync(task.Id, user2);
        Assert.Null(result);
    }
}
