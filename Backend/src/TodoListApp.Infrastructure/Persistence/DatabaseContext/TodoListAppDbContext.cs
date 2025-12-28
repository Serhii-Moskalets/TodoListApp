using Microsoft.EntityFrameworkCore;
using TodoListApp.Application.Abstractions.Interfaces.TodoListAppDbContext;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Infrastructure.Persistence.DatabaseContext;

/// <summary>
/// Represents the database context for the TodoList application.
/// Provides access to the database sets for all entities and applies their configurations.
/// </summary>
public class TodoListAppDbContext : DbContext, ITodoListAppDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TodoListAppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public TodoListAppDbContext(DbContextOptions<TodoListAppDbContext> options)
        : base(options) { }

    /// <summary>
    /// Gets or sets the DbSet of task lists.
    /// </summary>
    public DbSet<TaskListEntity> TaskLists { get; set; }

    /// <summary>
    /// Gets or sets the DbSet of tasks.
    /// </summary>
    public DbSet<TaskEntity> Tasks { get; set; }

    /// <summary>
    /// Gets or sets the DbSet of tags.
    /// </summary>
    public DbSet<TagEntity> Tags { get; set; }

    /// <summary>
    /// Gets or sets the DbSet of comments.
    /// </summary>
    public DbSet<CommentEntity> Comments { get; set; }

    /// <summary>
    /// Gets or sets the DbSet of users.
    /// </summary>
    public DbSet<UserEntity> Users { get; set; }

    /// <summary>
    /// Gets or sets the DbSet of user task accesses.
    /// </summary>
    public DbSet<UserTaskAccessEntity> UserTaskAccesses { get; set; }

    /// <summary>
    /// Configures the model by applying entity configurations from the assembly.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure the entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoListAppDbContext).Assembly);
    }
}
