using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Common.Extensions;
using TodoListApp.Application.UserTaskAccess.Mappers;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;

/// <summary>
/// Handles the <see cref="GetSharedTasksByUserIdQuery"/> by retrieving
/// all tasks that are shared with the specified user.
/// </summary>
public class GetSharedTasksByUserIdQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetSharedTasksByUserIdQuery, Result<PagedResultDto<TaskDto>>>
{
    /// <summary>
    /// Processes the query to retrieve tasks shared with a specific user.
    /// </summary>
    /// <param name="query">
    /// The query containing the identifier of the user whose shared tasks are requested.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a collection of <see cref="TaskDto"/>
    /// representing tasks shared with the user.
    /// </returns>
    public async Task<Result<PagedResultDto<TaskDto>>> Handle(GetSharedTasksByUserIdQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await this.UnitOfWork.UserTaskAccesses
            .GetSharedTasksByUserIdAsync(
            query.UserId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var result = items.ToPagedResult(totalCount, query.Page, query.PageSize, TaskAccessForUserMapper.Map);

        return await Result<PagedResultDto<TaskDto>>.SuccessAsync(result);
    }
}
