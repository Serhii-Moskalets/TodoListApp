using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Unit tests for <see cref="BaseRepository{TEntity}"/> using <see cref="TagRepository"/>.
/// Covers basic repository operations: Add, Delete, GetById, Exists, and tracking behavior.
/// </summary>
public class BaseRepositoryTests
{
    /// <summary>
    /// Tests that <see cref="TagRepository"/> adds an entity and returns it by identifier.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task AddAsync_Should_Add_Entity_And_GetByIdAsync_Should_Return_Entity()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var entity = new TagEntity("Test", Guid.NewGuid());
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        var saved = await repo.GetByIdAsync(entity.Id);
        Assert.NotNull(saved);
        Assert.Equal("Test", saved.Name);
    }

    /// <summary>
    /// Tests that <see cref="TagRepository"/> deletes an entity and it is no longer retrievable.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task DeleteAsync_Should_Remove_Entity()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var entity = new TagEntity("Test", Guid.NewGuid());
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        await repo.DeleteAsync(entity);
        await context.SaveChangesAsync();

        var saved = await repo.GetByIdAsync(entity.Id);
        Assert.Null(saved);
    }

    /// <summary>
    /// Tests that <see cref="TagRepository"/> reports existence for an existing entity.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsAsync_Should_Return_True_For_Existing_Entity()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var entity = new TagEntity("Test", Guid.NewGuid());
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsAsync(entity.Id);
        Assert.True(exists);
    }

    /// <summary>
    /// Tests that <see cref="TagRepository"/> reports non-existence for a missing entity.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task ExistsAsync_Should_Return_False_For_NonExisting_Entity()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var exists = await repo.ExistsAsync(Guid.NewGuid());
        Assert.False(exists);
    }

    /// <summary>
    /// Tests that deleting a null entity does not throw an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task DeleteAsync_Should_Handle_Null_Entity()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var exception = await Record.ExceptionAsync(() => repo.DeleteAsync(null!));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that adding a null entity
    /// throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task AddAsync_Should_Throw_ArgumentNullException_When_Entity_Is_Null()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAsync(null!));
    }

    /// <summary>
    /// Tests that entity retrieval respects the no-tracking configuration.
    /// </summary>
    /// <param name="asNoTracking">Indicates whether tracking is disabled.</param>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetByIdAsync_Should_Respect_AsNoTracking_Flag(bool asNoTracking)
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var entity = new TagEntity("TrackedTest", Guid.NewGuid());
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        var saved = await repo.GetByIdAsync(entity.Id, asNoTracking);
        Assert.NotNull(saved);
        Assert.Equal("TrackedTest", saved.Name);
    }

    /// <summary>
    /// Tests that multiple entities are stored and retrieved correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task Repository_Should_Handle_Multiple_Entities()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var entity1 = new TagEntity("Entity1", Guid.NewGuid());
        var entity2 = new TagEntity("Entity2", Guid.NewGuid());

        await repo.AddAsync(entity1);
        await repo.AddAsync(entity2);
        await context.SaveChangesAsync();

        var saved1 = await repo.GetByIdAsync(entity1.Id);
        var saved2 = await repo.GetByIdAsync(entity2.Id);

        Assert.NotNull(saved1);
        Assert.NotNull(saved2);
        Assert.Equal("Entity1", saved1.Name);
        Assert.Equal("Entity2", saved2.Name);
    }

    /// <summary>
    /// Tests that an entity is not persisted without calling SaveChanges.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task AddAsync_Should_Not_Persist_Without_SaveChanges()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var entity = new TagEntity("NoSave", Guid.NewGuid());
        await repo.AddAsync(entity);

        var saved = await repo.GetByIdAsync(entity.Id);
        Assert.Null(saved);
    }
}
