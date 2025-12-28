using Microsoft.Extensions.DependencyInjection;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Persistence.UnitOfWork;

namespace TodoListApp.Api.Extensions;

/// <summary>
/// Provides extension methods for registering persistence layer services
/// into the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the persistence-related services such as repositories
    /// and unit of work into the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register services into.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITaskListRepository, TaskListRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserTaskAccessRepository, UserTaskAccessRepository>();

        return services;
    }
}
