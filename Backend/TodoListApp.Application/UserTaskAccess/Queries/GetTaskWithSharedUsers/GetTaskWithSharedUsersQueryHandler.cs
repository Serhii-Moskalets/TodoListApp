using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.UserTaskAccess.Dto;
using TodoListApp.Application.UserTaskAccess.Mappers;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetTaskWithSharedUsers;

/// <summary>
/// Handles the <see cref="GetTaskWithSharedUsersQuery"/> and returns a list of users
/// who have shared access to a specific task.
/// </summary>
public class GetTaskWithSharedUsersQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IQueryHandler<GetTaskWithSharedUsersQuery, TaskAccessListDto>
{
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
        if (!await this.UnitOfWork.Tasks.IsTaskOwnerAsync(query.TaskId, query.UserId, cancellationToken))
        {
            return await Result<TaskAccessListDto>.FailureAsync(
                TinyResult.Enums.ErrorCode.InvalidOperation,
                "Only task owner can retrieve shared access.");
        }

        var utaEntityList = await this.UnitOfWork.UserTaskAccesses
            .GetSharedTasksByTaskIdAsync(query.TaskId, cancellationToken);

        return await Result<TaskAccessListDto>.SuccessAsync(
            TaskAccessForOwnerMapper.MapToTaskAccess(utaEntityList));
    }
}
