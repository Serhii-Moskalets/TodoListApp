using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteByTaskAccessByUserEmail;

/// <summary>
/// Handles the deletion of a user-task access entry based on the task ID and the user's email.
/// </summary>
public class DeleteByTaskAccessByUserEmailCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteByTaskAccessByUserEmailCommand>
{
    /// <summary>
    /// Handles the <see cref="DeleteByTaskAccessByUserEmailCommand"/> by checking if the access exists,
    /// deleting it if present, and returning the operation result.
    /// </summary>
    /// <param name="command">The command containing the task ID and user email.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was deleted,
    /// or failure if the access was not found.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteByTaskAccessByUserEmailCommand command, CancellationToken cancellationToken)
    {
        var accessExist = await this.UnitOfWork.UserTaskAccesses.ExistsTaskAccessWithEmail(command.TaskId, command.Email, cancellationToken);

        if (!accessExist)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, "Task access is not found.");
        }

        await this.UnitOfWork.UserTaskAccesses.DeleteByUserEmailAsync(command.TaskId, command.Email, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
