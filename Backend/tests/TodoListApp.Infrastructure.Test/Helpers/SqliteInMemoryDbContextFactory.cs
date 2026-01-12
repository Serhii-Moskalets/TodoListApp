using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;

namespace TodoListApp.Infrastructure.Test.Helpers;

/// <summary>
/// Factory for creating an in-memory SQLite database context
/// for integration and repository tests.
/// </summary>
/// <remarks>
/// Uses a persistent in-memory SQLite connection to ensure
/// relational behaviors (constraints, foreign keys, transactions)
/// are supported during testing.
/// </remarks>
public static class SqliteInMemoryDbContextFactory
{
    /// <summary>
    /// Creates and initializes a new <see cref="TodoListAppDbContext"/>
    /// backed by an in-memory SQLite database.
    /// </summary>
    /// <returns>
    /// A fully initialized <see cref="TodoListAppDbContext"/> instance
    /// with the database schema created.
    /// </returns>
    public static TodoListAppDbContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TodoListAppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TodoListAppDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
