using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="TagRepository"/>.
/// Tests cover existence checks, pagination and ownership verification.
/// </summary>
public class TagRepositoryTests
{
    /// <summary>
    /// Tests that checking tag existence by name returns false when the tag does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsByNameAsync_ReturnsFalse_WhenTagDoesNotExist()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        Assert.False(await repo.ExistsByNameAsync("NonExistingTag", userId));
    }

    /// <summary>
    /// Tests that <see cref="TagRepository.ExistsByNameAsync"/>
    /// returns true when a tag exists for a given user.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsByNameAsync_ReturnsTrue_WhenTagExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();
        var tag = new TagEntity("Tag", userId);

        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        Assert.True(await repo.ExistsByNameAsync("Tag", userId));
    }

    /// <summary>
    /// Tests that retrieving tags for a user returns an empty list when no tags exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetByUserIdAsync_ReturnsEmptyList_WhenNoTags()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        var result = await repo.GetByUserIdAsync(userId);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Checks that <see cref="TagRepository.GetByUserIdAsync"/>
    /// retrieves all tag lists for a given user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetByUserId_ReturnTagList_WhenTagsExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var userId = Guid.NewGuid();

        for (int i = 1; i <= 5; i++)
        {
            await repo.AddAsync(new TagEntity($"Tag_{i}", userId));
        }

        await context.SaveChangesAsync();

        var saved = await repo.GetByUserIdAsync(userId);
        Assert.Equal(5, saved.Count);
    }

    /// <summary>
    /// Tests that <see cref="TagRepository.GetPagedTagsByUserIdAsync"/>
    /// returns the correct page of tags and total count.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetPagedTagsByUserIdAsync_ReturnsCorrectPageAndCount()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        for (int i = 1; i <= 5; i++)
        {
            await repo.AddAsync(new TagEntity($"Tag_{i}", userId));
        }

        await context.SaveChangesAsync();

        var (items, total) = await repo.GetPagedTagsByUserIdAsync(
            userId,
            page: 2,
            pageSize: 2);

        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
        Assert.Equal("Tag_3", items.First().Name);
    }

    /// <summary>
    /// Tests that <see cref="TagRepository.IsTagOwnerAsync"/>
    /// returns correct ownership status for a tag.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task IsTagOwnerAsync_ReturnsCorrectValue()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId_1 = Guid.NewGuid();
        var userId_2 = Guid.NewGuid();
        var tag = new TagEntity("Tag", userId_1);
        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        Assert.True(await repo.IsTagOwnerAsync(tag.Id, userId_1));
        Assert.False(await repo.IsTagOwnerAsync(tag.Id, userId_2));
    }
}
