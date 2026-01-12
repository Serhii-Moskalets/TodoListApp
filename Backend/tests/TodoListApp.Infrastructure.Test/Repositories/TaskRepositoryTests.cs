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
            await repo.AddAsync(task);
        }

        await context.SaveChangesAsync();

        var count = await repo.CountOverdueTasksAsync(userId, taskListId, now);
        Assert.Equal(2, count);
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
        var now = DateTime.UtcNow.AddMinutes(5);

        var task_1 = new TaskEntity(userId, taskListId, "Task_1", now.AddMinutes(1));
        var task_3 = new TaskEntity(userId, taskListId, "Task_3", now.AddMinutes(2));
        var task_2 = new TaskEntity(userId, taskListId, "Task_2", now.AddMinutes(3));

        task_1.ChangeStatus(StatusTask.InProgress);
        task_3.ChangeStatus(StatusTask.InProgress);

        await repo.AddAsync(task_1);
        await repo.AddAsync(task_2);
        await repo.AddAsync(task_3);

        await context.SaveChangesAsync();

        var filtered = await repo.GetTasksAsync(
            userId,
            taskListId,
            statuses: [StatusTask.InProgress],
            sortBy: TaskSortBy.Title);

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

    /// <summary>
    /// Checks that <see cref="TaskRepository.GetTasksAsync"/>
    /// returns all tasks when statuses are null or empty.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTasks_ReturnsAll_WhenStatusesNullOrEmpty()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskRepository(context);
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var now = DateTime.UtcNow.AddMinutes(5);

        var tasks = new[]
        {
            new TaskEntity(userId, taskListId, "Task_1", now),
            new TaskEntity(userId, taskListId, "Task_2", now),
        };

        foreach (var task in tasks)
        {
            await repo.AddAsync(task);
        }

        await context.SaveChangesAsync();

        var allTasks1 = await repo.GetTasksAsync(userId, taskListId, statuses: null);
        var allTasks2 = await repo.GetTasksAsync(userId, taskListId, statuses: []);
        Assert.Equal(2, allTasks1.Count);
        Assert.Equal(2, allTasks2.Count);
    }

    /// <summary>
    /// Checks that <see cref="TaskRepository.GetTaskByIdForUserAsync"/>
    /// returns null when the task does not belong to the specified user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTaskByIdForUser_ReturnsNull_WhenUserMismatch()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TaskRepository(context);

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var task = new TaskEntity(user1, Guid.NewGuid(), "Task", DateTime.UtcNow.AddMinutes(5));
        await repo.AddAsync(task);
        await context.SaveChangesAsync();

        var result = await repo.GetTaskByIdForUserAsync(task.Id, user2);
        Assert.Null(result);
    }
}
