using TodoListApp.Domain.Entities;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Test.Helpers;

namespace TodoListApp.Infrastructure.Test.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task ExistsByEmail_ReturnTrue_WhenUserExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var user = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsByEmailAsync("john@example.com");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByUserName_ReturnTrue_WhenUserExists()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var user = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsByUserNameAsync("john");

        Assert.True(exists);
    }

    [Fact]
    public async Task GetByEmail_ReturnUser_WhenUserExisis()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var userEntity = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(userEntity);
        await context.SaveChangesAsync();

        var saved = await repo.GetByEmailAsync(userEntity.Email);

        Assert.NotNull(saved);
        Assert.Equal(saved.UserName, userEntity.UserName);
    }

    [Fact]
    public async Task GetByEmail_ReturnNull_WhenUserExisis()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var saved = await repo.GetByEmailAsync("john@example.com");

        Assert.Null(saved);
    }

    [Fact]
    public async Task GetByUserName_ReturnUser_WhenUserExisis()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var userEntity = new UserEntity("John", "john", "john@example.com", "hash");
        await context.Users.AddAsync(userEntity);
        await context.SaveChangesAsync();

        var saved = await repo.GetByUserNameAsync(userEntity.UserName);

        Assert.NotNull(saved);
        Assert.Equal(saved.UserName, userEntity.UserName);
    }

    [Fact]
    public async Task GetByUserName_ReturnNull_WhenUserExisis()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var repo = new UserRepository(context);
        var saved = await repo.GetByUserNameAsync("user");

        Assert.Null(saved);
    }
}
