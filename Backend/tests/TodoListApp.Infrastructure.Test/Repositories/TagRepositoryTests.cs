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
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        // Act & Assert
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
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();
        var tag = new TagEntity("Tag", userId);

        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        // Act & Assert
        Assert.True(await repo.ExistsByNameAsync("Tag", userId));
    }

    /// <summary>
    /// Tests that retrieving tags for a user returns an empty collection
    /// and zero count when no tags exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetTagsAsync_ReturnsEmptyResult_WhenNoTagsExist()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        // Act
        var (items, totalCount) = await repo.GetTagsAsync(userId, page: 1, pageSize: 10);

        // Assert
        Assert.Empty(items);
        Assert.Equal(0, totalCount);
    }

    /// <summary>
    /// Checks that <see cref="TagRepository.GetTagsAsync"/> retrieves
    /// a paginated list of tags and the correct total count for a user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTagsAsync_ReturnsPaginatedTagsAndTotalCount()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        for (int i = 1; i <= 15; i++)
        {
            await repo.AddAsync(new TagEntity($"Tag_{i:D2}", userId));
        }

        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetTagsAsync(userId, page: 1, pageSize: 10);

        // Assert
        Assert.Equal(10, items.Count);
        Assert.Equal(15, totalCount);
        Assert.Equal("Tag_01", items.First().Name);
        Assert.Equal("Tag_10", items.Last().Name);
    }

    /// <summary>
    /// Verifies that <see cref="TagRepository.GetTagsAsync"/> returns tags
    /// ordered by their creation date.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTagsAsync_ShouldReturnTagsOrderedByCreatedDate()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId = Guid.NewGuid();

        var tagOld = new TagEntity("Oldest", userId) { CreatedDate = DateTime.UtcNow.AddMinutes(-10) };
        var tagMiddle = new TagEntity("Middle", userId) { CreatedDate = DateTime.UtcNow.AddMinutes(-5) };
        var tagNew = new TagEntity("Newest", userId) { CreatedDate = DateTime.UtcNow };

        context.Tags.AddRange(tagMiddle, tagOld, tagNew);
        await context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await repo.GetTagsAsync(userId, page: 1, pageSize: 10);
        var itemsList = items.ToList();

        // Assert
        Assert.Equal(3, totalCount);
        Assert.Equal("Oldest", itemsList[0].Name);
        Assert.Equal("Middle", itemsList[1].Name);
        Assert.Equal("Newest", itemsList[2].Name);
    }

    /// <summary>
    /// Verifies that <see cref="TagRepository.GetTagByIdForUserAsync"/> returns the tag
    /// only if it belongs to the specified user.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetTagByIdForUserAsync_ReturnsTag_OnlyForCorrectOwner()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        var tag = new TagEntity("OwnerTag", ownerId);

        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        // Act
        var foundTag = await repo.GetTagByIdForUserAsync(tag.Id, ownerId);
        var notFoundTag = await repo.GetTagByIdForUserAsync(tag.Id, strangerId);

        // Assert
        Assert.NotNull(foundTag);
        Assert.Equal("OwnerTag", foundTag.Name);
        Assert.Null(notFoundTag);
    }

    /// <summary>
    /// Tests that <see cref="TagRepository.IsTagOwnerAsync"/>
    /// returns correct ownership status for a tag.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task IsTagOwnerAsync_ReturnsCorrectValue()
    {
        // Arrange
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);
        var userId_1 = Guid.NewGuid();
        var userId_2 = Guid.NewGuid();
        var tag = new TagEntity("Tag", userId_1);

        await repo.AddAsync(tag);
        await context.SaveChangesAsync();

        // Act & Assert
        Assert.True(await repo.IsTagOwnerAsync(tag.Id, userId_1));
        Assert.False(await repo.IsTagOwnerAsync(tag.Id, userId_2));
    }
}
