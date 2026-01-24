using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Common.Extensions;
using TodoListApp.Application.TaskList.Dtos;
using TodoListApp.Application.TaskList.Mappers;

namespace TodoListApp.Application.TaskList.Queries.GetTaskLists;

/// <summary>
/// Handles the <see cref="GetTaskListsQuery"/> by retrieving all task lists
/// for a specific user and mapping them to <see cref="TaskListDto"/> objects.
/// </summary>
public class GetTaskListsQueryHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetTaskListsQuery, Result<PagedResultDto<TaskListDto>>>
{
    /// <summary>
    /// Processes the query to get all task lists for a given user.
    /// </summary>
    /// <param name="query">The query containing the user ID.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a collection of <see cref="TaskListDto"/> objects.
    /// </returns>
    public async Task<Result<PagedResultDto<TaskListDto>>> Handle(GetTaskListsQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await this.UnitOfWork.TaskLists
            .GetTaskListsAsync(
            query.UserId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var result = items.ToPagedResult(totalCount, query.Page, query.PageSize, TaskListMapper.Map);

        return await Result<PagedResultDto<TaskListDto>>.SuccessAsync(result);
    }
}
