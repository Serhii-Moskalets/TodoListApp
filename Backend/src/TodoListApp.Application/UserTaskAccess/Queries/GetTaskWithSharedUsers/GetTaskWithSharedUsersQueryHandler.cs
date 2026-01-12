using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.UserTaskAccess.Dtos;
using TodoListApp.Application.UserTaskAccess.Mappers;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetTaskWithSharedUsers;

/// <summary>
/// Handles the <see cref="GetTaskWithSharedUsersQuery"/> and returns a list of users
/// who have shared access to a specific task.
/// </summary>
public class GetTaskWithSharedUsersQueryHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetTaskWithSharedUsersQuery> validator)
    : HandlerBase(unitOfWork), IQueryHandler<GetTaskWithSharedUsersQuery, TaskAccessListDto>
{
    private readonly IValidator<GetTaskWithSharedUsersQuery> _validator = validator;

    /// <summary>
    /// Handles the query to retrieve users with shared access to a task.
    /// </summary>
    /// <param name="query">The query containing the task ID and the requesting user ID.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{TaskAccessListDto}"/> containing the list of users with access,
    /// or a failure result if the requesting user is not the owner of the task.
    /// </returns>
    public async Task<Result<TaskAccessListDto>> Handle(GetTaskWithSharedUsersQuery query, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, query);
        if (!validation.IsSuccess)
        {
            return await Result<TaskAccessListDto>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(query.TaskId, query.UserId, asNoTracking: true, cancellationToken);
        if (task is null)
        {
            return await Result<TaskAccessListDto>.FailureAsync(ErrorCode.NotFound, "Task not found or you do not have permission.");
        }

        var utaEntityList = await this.UnitOfWork.UserTaskAccesses
            .GetUserTaskAccessByTaskIdAsync(query.TaskId, cancellationToken);

        return await Result<TaskAccessListDto>.SuccessAsync(new TaskAccessListDto
        {
            Id = task.Id,
            Title = task.Title,
            Users = TaskAccessForOwnerMapper.Map(utaEntityList),
        });
    }
}
