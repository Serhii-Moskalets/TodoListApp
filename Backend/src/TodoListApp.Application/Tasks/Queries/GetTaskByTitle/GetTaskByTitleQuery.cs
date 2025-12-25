using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;

namespace TodoListApp.Application.Tasks.Queries.GetTaskByTitle;

/// <summary>
/// Query to retrieve a task by its title (or partial title) for a specific user.
/// </summary>
public sealed record GetTaskByTitleQuery(Guid UserId, string Text)
    : IQuery<IEnumerable<TaskDto>>;
