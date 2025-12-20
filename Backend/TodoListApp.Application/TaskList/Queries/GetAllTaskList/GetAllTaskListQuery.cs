using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.TaskList.Dtos;

namespace TodoListApp.Application.TaskList.Queries.GetAllTaskList;

/// <summary>
/// Represents a query to retrieve all task lists for a specific user.
/// </summary>
public record GetAllTaskListQuery(Guid UserId)
    : IQuery<IEnumerable<TaskListDto>> { }
