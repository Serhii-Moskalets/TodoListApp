using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="TaskRepository"/>.
/// Tests cover existence checks, overdue task handling, pagination, filtering, and ownership verification.
/// </summary>
public class TaskRepositoryTests
{
    /// <summary>
    /// Tests that <see cref="TaskRepository.ExistsForUserAsync"/>
    /// returns true when a task exists for a given user.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsForUser_ReturnsTrue_WhenTaskExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskRepository(context);
        var userId = Guid.NewGuid();
        var task = new TaskEntity(userId, taskListId: Guid.NewGuid(), "Task", DateTime.UtcNow.AddDays(1));
        await repo.AddAsync(task);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsForUserAsync(task.Id, userId);
        Assert.True(exists);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.ExistsForUserAsync"/>
    /// returns false when the task does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsForUser_ReturnsFalse_WhenTaskDoesNotExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskRepository(context);
        var userId = Guid.NewGuid();

        var exists = await repo.ExistsForUserAsync(Guid.NewGuid(), userId);
        Assert.False(exists);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.CountOverdueTasksAsync"/>
    /// returns the correct number of overdue tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task CountOverdueTask_ReturnsCorrectCount()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskRepository(context);
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var tasks = new[]
        {
            new TaskEntity(userId, taskListId, "Task_1", now.AddDays(-1)),
            new TaskEntity(userId, taskListId, "Task_2", now.AddDays(-2)),
            new TaskEntity(userId, taskListId, "Task_3", now.AddDays(1)),
        };

        foreach (var task in tasks)
        {
            await repo.AddAsync(task);
        }

        await context.SaveChangesAsync();

        var count = await repo.CountOverdueTasksAsync(userId, taskListId, now);
        Assert.Equal(2, count);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.DeleteOverdueTasksAsync"/> removes only overdue tasks.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task DeleteOverdueTasks_RemovesOnlyOverdueTasks()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskRepository(context);
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var tasks = new[]
        {
            new TaskEntity(userId, taskListId, "Task_1", now.AddDays(-1)),
            new TaskEntity(userId, taskListId, "Task_2", now.AddDays(-2)),
            new TaskEntity(userId, taskListId, "Task_3", now.AddDays(1)),
        };

        foreach (var task in tasks)
        {
            await repo.AddAsync(task);
        }

        await context.SaveChangesAsync();

        await repo.DeleteOverdueTasksAsync(userId, taskListId, now);
        await context.SaveChangesAsync();

        var allTasks = await repo.GetTasksAsync(userId, taskListId);
        Assert.Single(allTasks);
        Assert.Equal("Task_3", allTasks.First().Title);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.GetPaginatedAsync"/>
    /// returns the correct page of tasks and total count.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetPaginatedTasks_ReturnsCorrectPageAndCount()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskRepository(context);
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        for (int i = 1; i <= 5; i++)
        {
            await repo.AddAsync(new TaskEntity(userId, taskListId, $"Task_{i}", DateTime.UtcNow.AddDays(i)));
        }

        await context.SaveChangesAsync();

        var (items, total) = await repo.GetPaginatedAsync(
            userId,
            taskListId,
            page: 2,
            pageSize: 2);
        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
        Assert.Equal("Task_3", items.First().Title);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.GetTasksAsync"/> can filter by status and sort by title.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetTasks_CanFilterAndSort()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskRepository(context);
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var task_1 = new TaskEntity(userId, taskListId, "Task_1", now.AddDays(-1));
        var task_3 = new TaskEntity(userId, taskListId, "Task_3", now.AddDays(1));
        var task_2 = new TaskEntity(userId, taskListId, "Task_2", now.AddDays(-2));

        task_1.ChangeStatus(StatusTask.InProgress);
        task_3.ChangeStatus(StatusTask.InProgress);

        await repo.AddAsync(task_1);
        await repo.AddAsync(task_2);
        await repo.AddAsync(task_3);

        await context.SaveChangesAsync();

        var filtered = await repo.GetTasksAsync(userId, taskListId, statuses: [StatusTask.InProgress], sortBy: TaskSortBy.Title);
        Assert.Equal(2, filtered.Count);
        Assert.Equal(["Task_1", "Task_3"], [.. filtered.Select(x => x.Title)]);
    }

    /// <summary>
    /// Tests that <see cref="TaskRepository.IsTaskOwnerAsync"/>
    /// returns correct ownership status for a task.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task IsOwnerTask_ReturnsCorrectValue()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskRepository(context);
        var userId_1 = Guid.NewGuid();
        var userId_2 = Guid.NewGuid();
        var task = new TaskEntity(userId_1, Guid.NewGuid(), "Task");
        await repo.AddAsync(task);
        await context.SaveChangesAsync();

        Assert.True(await repo.IsTaskOwnerAsync(task.Id, userId_1));
        Assert.False(await repo.IsTaskOwnerAsync(task.Id, userId_2));
    }
}
