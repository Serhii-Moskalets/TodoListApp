using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.UserTaskAccess.Mappers;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;

/// <summary>
/// Handles the <see cref="GetSharedTasksByUserIdQuery"/> by retrieving
/// all tasks that are shared with the specified user.
/// </summary>
public class GetSharedTasksByUserIdQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IQueryHandler<GetSharedTasksByUserIdQuery, IEnumerable<TaskDto>>
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
    public async Task<Result<IEnumerable<TaskDto>>> Handle(GetSharedTasksByUserIdQuery query, CancellationToken cancellationToken)
    {
        var sharedTaskEntities = await this.UnitOfWork.UserTaskAccesses
            .GetSharedTasksByUserIdAsync(query.UserId, cancellationToken);

        return await Result<IEnumerable<TaskDto>>.SuccessAsync(
            TaskAccessForUserMapper.Map(sharedTaskEntities));
    }
}
