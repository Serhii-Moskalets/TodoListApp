using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Persistence.UnitOfWork;

namespace TodoListApp.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for registering infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the infrastructure services to the specified <see cref="IServiceCollection"/>,
    /// including the <see cref="TodoListAppDbContext"/> configured to use PostgreSQL.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionString">The connection string to the PostgreSQL database.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TodoListAppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // --- Add all repository ---
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITaskListRepository, TaskListRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserTaskAccessRepository, UserTaskAccessRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
