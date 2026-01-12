using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.UserTaskAccess.Mappers;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTaskById;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;

/// <summary>
/// Handles the <see cref="GetSharedTasksByUserIdQuery"/> by retrieving
/// all tasks that are shared with the specified user.
/// </summary>
public class GetSharedTasksByUserIdQueryHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetSharedTasksByUserIdQuery> validator)
    : HandlerBase(unitOfWork), IQueryHandler<GetSharedTasksByUserIdQuery, IEnumerable<TaskDto>>
{
    private readonly IValidator<GetSharedTasksByUserIdQuery> _validator = validator;

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
        var validation = await ValidateAsync(this._validator, query);
        if (!validation.IsSuccess)
        {
            return await Result<IEnumerable<TaskDto>>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var sharedTaskEntities = await this.UnitOfWork.UserTaskAccesses
            .GetSharedTasksByUserIdAsync(query.UserId, cancellationToken);

        return await Result<IEnumerable<TaskDto>>.SuccessAsync(
            TaskAccessForUserMapper.Map(sharedTaskEntities));
    }
}
