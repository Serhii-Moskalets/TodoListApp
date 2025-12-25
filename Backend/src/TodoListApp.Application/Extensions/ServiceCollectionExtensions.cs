using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Comment.Commands.CreateComment;
using TodoListApp.Application.Comment.Commands.DeleteComment;
using TodoListApp.Application.Comment.Commands.UpdateComment;
using TodoListApp.Application.Comment.Queries.GetComments;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tag.Commands.CreateTag;
using TodoListApp.Application.Tag.Commands.DeleteTag;
using TodoListApp.Application.Tag.Queries.GetAllTags;
using TodoListApp.Application.TaskList.Commands.CreateTaskList;
using TodoListApp.Application.TaskList.Commands.DeleteTaskList;
using TodoListApp.Application.TaskList.Commands.UpdateTaskList;
using TodoListApp.Application.TaskList.Dtos;
using TodoListApp.Application.TaskList.Queries.GetAllTaskList;
using TodoListApp.Application.Tasks.Commands.AddTagToTask;
using TodoListApp.Application.Tasks.Commands.ChangeTaskStatus;
using TodoListApp.Application.Tasks.Commands.CreateTask;
using TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;
using TodoListApp.Application.Tasks.Commands.DeleteTask;
using TodoListApp.Application.Tasks.Commands.RemoveTagFromTask;
using TodoListApp.Application.Tasks.Commands.UpdateTask;
using TodoListApp.Application.Tasks.Queries.GetTaskById;
using TodoListApp.Application.Tasks.Queries.GetTaskByTitle;
using TodoListApp.Application.Tasks.Queries.GetTasks;
using TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteAllTaskAccessesByTaskId;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteAllTaskAccessesByUserId;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteByTaskAccessByUserEmail;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;
using TodoListApp.Application.UserTaskAccess.Dto;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTaskById;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;
using TodoListApp.Application.UserTaskAccess.Queries.GetTaskWithSharedUsers;

namespace TodoListApp.Application.Extensions;

/// <summary>
/// Provides extension methods to register all application-level services,
/// including FluentValidation validators and CQRS command/query handlers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services into the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    public static void AddApplicationServices(this IServiceCollection services)
    {
        AddApplicationValidators(services);
        AddApplicationHandlers(services);
    }

    /// <summary>
    /// Registers all FluentValidation validators used in the application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddApplicationValidators(this IServiceCollection services)
    {
        // --- Tag Validators ---
        services.AddScoped<IValidator<CreateTagCommand>, CreateTagCommandValidator>();
        services.AddScoped<IValidator<DeleteTagCommand>, DeleteTagCommandValidator>();

        // --- Comment Validators ---
        services.AddScoped<IValidator<CreateCommentCommand>, CreateCommentCommandValidator>();
        services.AddScoped<IValidator<DeleteCommentCommand>, DeleteCommentCommandValidator>();
        services.AddScoped<IValidator<UpdateCommentCommand>, UpdateCommentCommandValidator>();

        // --- Task Validators ---
        services.AddScoped<IValidator<CreateTaskCommand>, CreateTaskCommandValidator>();
        services.AddScoped<IValidator<DeleteOverdueTasksCommand>, DeleteOverdueTasksCommandValidator>();
        services.AddScoped<IValidator<DeleteTaskCommand>, DeleteTaskCommandValidator>();
        services.AddScoped<IValidator<UpdateTaskCommand>, UpdateTaskCommandValidator>();

        // --- TaskList Validators ---
        services.AddScoped<IValidator<CreateTaskListCommand>, CreateTaskListCommandValidator>();
        services.AddScoped<IValidator<DeleteTaskListCommand>, DeleteTaskListCommandValidator>();
        services.AddScoped<IValidator<UpdateTaskListCommand>, UpdateTaskListCommandValidator>();

        // --- UserTaskAccess Validators ---
        services.AddScoped<IValidator<CreateUserTaskAccessCommand>, CreateUserTaskAccessCommandValidator>();
        services.AddScoped<IValidator<DeleteTaskAccessByUserEmailCommand>, DeleteTaskAccessByUserEmailCommandValidator>();
        services.AddScoped<IValidator<DeleteTaskAccessByIdCommand>, DeleteTaskAccessByIdCommandValidator>();
    }

    /// <summary>
    /// Registers all command and query handlers used in the application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddApplicationHandlers(this IServiceCollection services)
    {
        // --- Tag Handlers ---
        services.AddScoped<ICommandHandler<CreateTagCommand>, CreateTagCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTagCommand>, DeleteTagCommandHandler>();
        services.AddScoped<IQueryHandler<GetAllTagsQuery, IEnumerable<TagDto>>, GetAllTagsQueryHandler>();

        // --- Comment Handlers ---
        services.AddScoped<ICommandHandler<CreateCommentCommand>, CreateCommentCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteCommentCommand>, DeleteCommentCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCommentCommand>, UpdateCommentCommandHandler>();
        services.AddScoped<IQueryHandler<GetCommentsQuery, IEnumerable<CommentDto>>, GetCommentsQueryHandler>();

        // --- Task Handlers ---
        services.AddScoped<ICommandHandler<AddTagToTaskCommand>, AddTagToTaskCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeTaskStatusCommand>, ChangeTaskStatusCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteOverdueTasksCommand>, DeleteOverdueTasksCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTaskCommand>, DeleteTaskCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveTagFromTaskCommand>, RemoveTagFromTaskCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateTaskCommand>, UpdateTaskCommandHandler>();
        services.AddScoped<IQueryHandler<GetTaskByIdQuery, TaskDto>, GetTaskByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetTaskByTitleQuery, IEnumerable<TaskDto>>, GetTaskByTitleQueryHandler>();
        services.AddScoped<IQueryHandler<GetTasksQuery, IEnumerable<TaskDto>>, GetTasksQueryHandler>();

        // --- TaskList Handlers ---
        services.AddScoped<ICommandHandler<CreateTaskListCommand>, CreateTaskListCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTaskListCommand>, DeleteTaskListCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateTaskListCommand>, UpdateTaskListCommandHandler>();
        services.AddScoped<IQueryHandler<GetAllTaskListQuery, IEnumerable<TaskListDto>>, GetAllTaskListQueryHandler>();

        // --- UserTaskAccess Handlers ---
        services.AddScoped<ICommandHandler<CreateUserTaskAccessCommand>, CreateUserTaskAccessCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteAllTaskAccessesByTaskIdCommand>, DeleteAllTaskAccessesByTaskIdCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteAllTaskAccessesByUserIdCommand>, DeleteAllTaskAccessesByUserIdCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTaskAccessByUserEmailCommand>, DeleteTaskAccessByUserEmailCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTaskAccessByIdCommand>, DeleteTaskAccessByIdCommandHandler>();
        services.AddScoped<IQueryHandler<GetSharedTaskByIdQuery, TaskDto>, GetSharedTaskByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetTaskWithSharedUsersQuery, TaskAccessListDto>, GetTaskWithSharedUsersQueryHandler>();
        services.AddScoped<IQueryHandler<GetSharedTasksByUserIdQuery, IEnumerable<TaskDto>>, GetSharedTasksByUserIdQueryHandler>();
    }
}
