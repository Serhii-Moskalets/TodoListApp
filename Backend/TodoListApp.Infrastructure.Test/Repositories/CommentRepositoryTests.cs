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
    /// Tests that <see cref="CommentRepository.GetByTaskIdAsync"/> returns all comments
    /// associated with a specific task when comments exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetByTaskId_WhenCommentExists()
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

        var saved = await repo.GetByTaskIdAsync(taskId);
        Assert.NotNull(saved);
        Assert.Equal(5, saved.Count);
    }

    /// <summary>
    /// Tests that <see cref="CommentRepository.IsCommentOwnerAsync"/> correctly determines
    /// if a user is the owner of a comment.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task IsCommentOwner_ReturnsCorrectValue()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new CommentRepository(context);
        var userId_1 = Guid.NewGuid();
        var userId_2 = Guid.NewGuid();
        var comment = new CommentEntity(Guid.NewGuid(), userId_1, "Text");

        await repo.AddAsync(comment);
        await context.SaveChangesAsync();

        Assert.True(await repo.IsCommentOwnerAsync(comment.Id, userId_1));
        Assert.False(await repo.IsCommentOwnerAsync(comment.Id, userId_2));
    }

    /// <summary>
    /// Tests that <see cref="CommentRepository.GetPagedCommentsByTaskIdAsync"/> returns
    /// the correct page of comments along with the total count of comments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPaginatedComments_ReturnsCorrectPageAndCount()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new CommentRepository(context);
        var user = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user);

        var taskid = Guid.NewGuid();

        for (int i = 1; i <= 5; i++)
        {
            await repo.AddAsync(new CommentEntity(taskid, user.Id, $"Text_{i}"));
        }

        await context.SaveChangesAsync();

        var (items, total) = await repo.GetPagedCommentsByTaskIdAsync(taskid, page: 1, pageSize: 2);
        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
        Assert.Equal("Text_1", items.First().Text);
    }
}
