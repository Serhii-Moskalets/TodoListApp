using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.UserTaskAccess.Mappers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTaskById;

/// <summary>
/// Handles retrieval of a task shared with a specific user.
/// </summary>
public class GetSharedTaskByIdQueryHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetSharedTaskByIdQuery> validator)
    : HandlerBase(unitOfWork), IQueryHandler<GetSharedTaskByIdQuery, TaskDto>
{
    private readonly IValidator<GetSharedTaskByIdQuery> _validator = validator;

    /// <summary>
    /// Handles the <see cref="GetSharedTaskByIdQuery"/> request.
    /// </summary>
    /// <param name="query">
    /// The query containing the task identifier and the user identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the shared task data if the user has access;
    /// otherwise, a failure result indicating that the task was not found or access is denied.
    /// </returns>
    public async Task<Result<TaskDto>> Handle(GetSharedTaskByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, query);
        if (!validation.IsSuccess)
        {
            return await Result<TaskDto>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var access = await this.UnitOfWork.UserTaskAccesses
            .GetByTaskAndUserIdAsync(query.TaskId, query.UserId, cancellationToken);

        if (access is null)
        {
            return await Result<TaskDto>.FailureAsync(
                TinyResult.Enums.ErrorCode.NotFound,
                "Task not found.");
        }

        return await Result<TaskDto>.SuccessAsync(
            TaskAccessForUserMapper.Map(access));
    }
}
