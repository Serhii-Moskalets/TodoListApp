using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;

namespace TodoListApp.Application.Task.Queries.GetTaskByTitle;

/// <summary>
/// Represents a query to retrieve a task by its title (or partial title) for a specific user.
/// </summary>
/// <param name="UserId">The identifier of the user who owns the task.</param>
/// <param name="Text">The title or text to search for in the task's title.</param>
public sealed record GetTaskByTitleQuery(Guid UserId, string Text)
    : IQuery<IEnumerable<TaskDto>>;
