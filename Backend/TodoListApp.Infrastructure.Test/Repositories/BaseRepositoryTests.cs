using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

/// <summary>
/// Contains unit tests for <see cref="BaseRepository{TEntity}"/> functionality
/// using the <see cref="TagRepository"/> as a concrete example.
/// Tests include adding, retrieving, deleting, and checking existence of entities.
/// </summary>
public class BaseRepositoryTests
{
    /// <summary>
    /// Tests that an entity can be added to the repository and then retrieved by ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Can_Add_And_Get_Entity()
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
    /// Tests that an entity can be added and then deleted from the repository.
    /// Verifies that the entity no longer exists after deletion.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Can_Add_And_Delete_Entity()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var entity = new TagEntity("Test", Guid.NewGuid());
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        await repo.DeleteAsync(entity.Id);
        await context.SaveChangesAsync();

        var saved = await repo.GetByIdAsync(entity.Id);
        Assert.Null(saved);
    }

    /// <summary>
    /// Tests that the <see cref="BaseRepository{TEntity}.ExistsAsync"/> method
    /// correctly reports the existence of an entity in the repository.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Can_Add_And_Check_Exist_Entity()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new TagRepository(context);

        var entity = new TagEntity("Test", Guid.NewGuid());
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsAsync(entity.Id);
        Assert.True(exists);
    }
}
