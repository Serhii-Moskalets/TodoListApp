using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Mappers;

namespace TodoListApp.Application.Tasks.Queries.GetTasks;

/// <summary>
/// Handles the <see cref="GetTasksQuery"/> to retrieve a filtered and sorted list of tasks for a specific user.
/// </summary>
public class GetTasksQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetTasksQuery, Result<PagedResultDto<TaskDto>>>
{
    /// <summary>
    /// Retrieves tasks based on the provided filters and sorting options, maps them to <see cref="TaskDto"/>,
    /// and returns the result wrapped in a <see cref="TinyResult.Result{T}"/>.
    /// </summary>
    /// <param name="query">The query containing user ID, optional task statuses, due date filters, and sorting options.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> representing the outcome of the operation.
    /// </returns>
    public async Task<Result<PagedResultDto<TaskDto>>> Handle(GetTasksQuery query, CancellationToken cancellationToken)
    {
        var (taskEntities, totalCount) = await this.UnitOfWork.Tasks.GetTasksAsync(
            query.UserId,
            query.TaskListId,
            query.Page,
            query.PageSize,
            query.TaskStatuses,
            query.DueBefore,
            query.DueAfter,
            query.TaskSortBy,
            query.Ascending,
            cancellationToken);

        var taskDtoList = TaskMapper.Map(taskEntities);

        var result = new PagedResultDto<TaskDto>
        {
            Items = taskDtoList,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };

        return await Result<PagedResultDto<TaskDto>>.SuccessAsync(result);
    }
}
