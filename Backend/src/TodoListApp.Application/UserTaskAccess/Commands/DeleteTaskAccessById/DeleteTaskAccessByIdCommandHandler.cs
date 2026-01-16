using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

/// <summary>
/// Handles the <see cref="DeleteTaskAccessByIdCommand"/> to remove a user-task access entry.
/// </summary>
public class DeleteTaskAccessByIdCommandHandler(
    IUnitOfWork unitOfWork,
    IUserTaskAccessService userTaskAccessService)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteTaskAccessByIdCommand, Result<bool>>
{
    private readonly IUserTaskAccessService _userTaskAccessService = userTaskAccessService;

    /// <summary>
    /// Processes the command to delete a user-task access entry.
    /// </summary>
    /// <param name="command">
    /// The command containing the ID of the task and the user ID whose access should be removed.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was deleted,
    /// or failure if the access was not found.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteTaskAccessByIdCommand command, CancellationToken cancellationToken)
    {
        var hasAccess = await this._userTaskAccessService.HasAccessAsync(command.TaskId, command.UserId, cancellationToken);

        if (!hasAccess)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.ValidationError, "User hasn't accesss with this task.");
        }

        await this.UnitOfWork.UserTaskAccesses.DeleteByIdAsync(command.TaskId, command.UserId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
