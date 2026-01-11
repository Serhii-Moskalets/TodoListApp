using Microsoft.EntityFrameworkCore;
using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Tests for <see cref="UserTaskAccessRepository"/>.
/// Verifies adding, deleting, checking existence, and retrieving user-task access records.
/// </summary>
public class UserTaskAccessRepositoryTests
{
    /// <summary>
    /// Verifies that a user-task access entry can be added
    /// and then retrieved by task ID and user ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task Add_GetByTaskAndUserIdAsync_ShouldAddUserTaskAccessAndGetByTaskAndUserId()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", "hash2");
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, "Task list 1");
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user_1.Id, taskList.Id, "Task1");
        await context.Tasks.AddAsync(task);

        var access = new UserTaskAccessEntity(task.Id, user_2.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        var saved = await repo.GetByTaskAndUserIdAsync(task.Id, user_2.Id);

        Assert.NotNull(saved);
        Assert.Equal(task.Id, saved.TaskId);
        Assert.Equal(user_2.Id, saved.UserId);
    }

    /// <summary>
    /// Verifies that a specific user-task access entry
    /// can be removed by task ID and user ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task DeleteByTaskAndUserIdAsync_ShouldRemoveAccess()
    {
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", "hash2");
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, "Task list 1");
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user_1.Id, taskList.Id, "Task1");
        await context.Tasks.AddAsync(task);

        var access = new UserTaskAccessEntity(task.Id, user_2.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        var deleted = await repo.DeleteByIdAsync(task.Id, user_2.Id);

        Assert.Equal(1, deleted);
        Assert.False(await context.UserTaskAccesses.AnyAsync());
    }

    /// <summary>
    /// Verifies that all access entries related to a specific task
    /// are removed when deleting by task ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task DeleteAllByTaskIdAsync_ShouldRemoveAccess()
    {
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", "hash2");
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, "Task list 1");
        await context.TaskLists.AddAsync(taskList);

        var task_1 = new TaskEntity(user_1.Id, taskList.Id, "Task 1");
        await context.Tasks.AddAsync(task_1);

        var task_2 = new TaskEntity(user_2.Id, taskList.Id, "Task 2");
        await context.Tasks.AddAsync(task_2);

        var access_1 = new UserTaskAccessEntity(task_1.Id, user_2.Id);
        await repo.AddAsync(access_1);

        var access_2 = new UserTaskAccessEntity(task_2.Id, user_1.Id);
        await repo.AddAsync(access_2);
        await context.SaveChangesAsync();

        var deleted = await repo.DeleteAllByTaskIdAsync(task_1.Id);

        Assert.Equal(1, deleted);
        Assert.False(await repo.ExistsAsync(task_1.Id, user_2.Id));
        Assert.True(await repo.ExistsAsync(task_2.Id, user_1.Id));
    }

    /// <summary>
    /// Verifies that all access entries related to a specific user
    /// are removed when deleting by user ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task DeleteAllByUserIdAsync_ShouldRemoveAccess()
    {
        await using var context = SqliteInMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", "hash2");
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, "Task list 1");
        await context.TaskLists.AddAsync(taskList);

        var task_1 = new TaskEntity(user_1.Id, taskList.Id, "Task 1");
        await context.Tasks.AddAsync(task_1);

        var task_2 = new TaskEntity(user_2.Id, taskList.Id, "Task 2");
        await context.Tasks.AddAsync(task_2);

        var access_1 = new UserTaskAccessEntity(task_1.Id, user_2.Id);
        await repo.AddAsync(access_1);

        var access_2 = new UserTaskAccessEntity(task_2.Id, user_1.Id);
        await repo.AddAsync(access_2);
        await context.SaveChangesAsync();

        var deleted = await repo.DeleteAllByUserIdAsync(user_2.Id);

        Assert.Equal(1, deleted);
        Assert.False(await repo.ExistsAsync(task_1.Id, user_2.Id));
        Assert.True(await repo.ExistsAsync(task_2.Id, user_1.Id));
    }

    /// <summary>
    /// Verifies that all shared access entries for a task
    /// are returned correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetSharedTasksByTaskIdAsync_ShouldReturnAccessList()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John1", "john3", "john1@example.com", "hash3");
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", "hash2");
        await context.Users.AddAsync(user_2);
        var user_3 = new UserEntity("John3", "john1", "john3@example.com", "hash1");
        await context.Users.AddAsync(user_3);

        var taskList = new TaskListEntity(user_1.Id, "Task list 1");
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user_1.Id, taskList.Id, "Task1");
        await context.Tasks.AddAsync(task);

        var access_1 = new UserTaskAccessEntity(task.Id, user_2.Id);
        var access_2 = new UserTaskAccessEntity(task.Id, user_3.Id);
        await repo.AddAsync(access_1);
        await repo.AddAsync(access_2);
        await context.SaveChangesAsync();

        var shared = await repo.GetUserTaskAccessByTaskIdAsync(task.Id);
        Assert.Equal(2, shared.Count);
        Assert.Contains(shared, x => x.UserId == user_2.Id);
        Assert.Contains(shared, x => x.UserId == user_3.Id);
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessRepository.GetSharedTasksByUserIdAsync"/>
    /// returns all tasks shared with a specific user.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetSharedTasksByUserIdAsync_ShouldReturnCorrectTasks()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var owner = new UserEntity("Owner", "owner", "owner@example.com", "hash");
        var sharedUser = new UserEntity("Shared", "shared", "shared@example.com", "hash2");
        await context.Users.AddRangeAsync(owner, sharedUser);

        var taskList = new TaskListEntity(owner.Id, "List");
        await context.TaskLists.AddAsync(taskList);

        var task1 = new TaskEntity(owner.Id, taskList.Id, "Task1");
        var task2 = new TaskEntity(owner.Id, taskList.Id, "Task2");
        await context.Tasks.AddRangeAsync(task1, task2);

        var access1 = new UserTaskAccessEntity(task1.Id, sharedUser.Id);
        var access2 = new UserTaskAccessEntity(task2.Id, sharedUser.Id);
        await repo.AddAsync(access1);
        await repo.AddAsync(access2);
        await context.SaveChangesAsync();

        var sharedTasks = await repo.GetSharedTasksByUserIdAsync(sharedUser.Id);
        Assert.Equal(2, sharedTasks.Count);
        Assert.All(sharedTasks, t => Assert.Equal(sharedUser.Id, t.UserId));
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessRepository.ExistsAsync"/>
    /// returns true when an access entry exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenAccessExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", "hash2");
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, "Task list 1");
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user_1.Id, taskList.Id, "Task1");
        await context.Tasks.AddAsync(task);

        var access = new UserTaskAccessEntity(task.Id, user_2.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        Assert.True(await repo.ExistsAsync(task.Id, user_2.Id));
    }

    /// <summary>
    /// Verifies that <see cref="UserTaskAccessRepository.ExistsAsync"/>
    /// returns <c>false</c> when no matching user-task access exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenAccessDoesNotExist()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserTaskAccessRepository(context);

        var user_1 = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user_1);
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", "hash2");
        await context.Users.AddAsync(user_2);

        var taskList = new TaskListEntity(user_1.Id, "Task list 1");
        await context.TaskLists.AddAsync(taskList);

        var task = new TaskEntity(user_1.Id, taskList.Id, "Task1");
        await context.Tasks.AddAsync(task);

        var access = new UserTaskAccessEntity(task.Id, user_2.Id);
        await repo.AddAsync(access);
        await context.SaveChangesAsync();

        Assert.False(await repo.ExistsAsync(Guid.NewGuid(), user_2.Id));
        Assert.False(await repo.ExistsAsync(task.Id, Guid.NewGuid()));
    }
}
