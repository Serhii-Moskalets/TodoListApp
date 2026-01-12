using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Contains unit tests for <see cref="CommentRepository"/>.
/// </summary>
public class CommentRepositoryTests
{
    /// <summary>
    /// Tests that retrieving comments for a task with no comments returns an empty list.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetByTaskIdAsync_WhenNoComments_ReturnsEmptyList()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new CommentRepository(context);

        var taskId = Guid.NewGuid();
        var result = await repo.GetByTaskIdAsync(taskId);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that requesting paged comments with an invalid page number
    /// or page size throws <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    /// <param name="page">The page number to request.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 0)]
    [InlineData(-1, 2)]
    [InlineData(1, -5)]
    public async Task GetPagedCommentsByTaskIdAsync_InvalidPageOrPageSize_Throws(int page, int pageSize)
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new CommentRepository(context);
        var taskId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            repo.GetPagedCommentsByTaskIdAsync(taskId, page, pageSize));
    }

    /// <summary>
    /// Tests that retrieving comments for a task includes the related user entity.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetByTaskIdAsync_IncludesUser()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new CommentRepository(context);

        var user = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var taskId = Guid.NewGuid();
        var comment = new CommentEntity(taskId, user.Id, "Text");
        await repo.AddAsync(comment);
        await context.SaveChangesAsync();

        var saved = await repo.GetByTaskIdAsync(taskId);
        Assert.Single(saved);
        Assert.NotNull(saved.First().User);
        Assert.Equal("john", saved.First().User.UserName);
    }

    /// <summary>
    /// Tests that retrieving the second page of comments returns the correct items and total count.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetPagedCommentsByTaskIdAsync_PageTwo_ReturnsCorrectItems()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new CommentRepository(context);

        var user = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user);

        var taskId = Guid.NewGuid();
        for (int i = 1; i <= 5; i++)
        {
            await repo.AddAsync(new CommentEntity(taskId, user.Id, $"Text_{i}"));
        }

        await context.SaveChangesAsync();

        var (items, total) = await repo.GetPagedCommentsByTaskIdAsync(taskId, page: 2, pageSize: 2);
        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
        Assert.Equal("Text_3", items.First().Text);
    }
}
