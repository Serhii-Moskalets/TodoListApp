using Microsoft.EntityFrameworkCore;
using TodoListApp.Infrastructure.Persistence;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;

namespace TodoListApp.Infrastructure.Test.Helpers;

/// <summary>
/// Provides a factory to create instances of <see cref="TodoListAppDbContext"/>
/// configured to use an in-memory database for testing purposes.
/// </summary>
public static class InMemoryDbContextFactory
{
    /// <summary>
    /// Creates a new <see cref="TodoListAppDbContext"/> instance
    /// using a unique in-memory database.
    /// </summary>
    /// <returns>A <see cref="TodoListAppDbContext"/> configured for in-memory usage.</returns>
    public static TodoListAppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TodoListAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TodoListAppDbContext(options);
    }
}
