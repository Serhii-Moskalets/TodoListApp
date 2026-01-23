using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.TaskList.Dtos;

namespace TodoListApp.Application.TaskList.Queries.GetTaskLists;

/// <summary>
/// Represents a query to retrieve all task lists for a specific user.
/// </summary>
public record GetTaskListsQuery(Guid UserId, int Page = 1, int PageSize = 10)
    : IQuery<PagedResultDto<TaskListDto>>;
