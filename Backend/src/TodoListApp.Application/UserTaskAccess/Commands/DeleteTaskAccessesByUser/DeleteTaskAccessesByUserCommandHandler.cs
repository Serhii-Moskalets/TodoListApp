using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;

/// <summary>
/// Handles the <see cref="DeleteTaskAccessesByUserCommand"/> to remove all user-task access entries for a specific user.
/// </summary>
public class DeleteTaskAccessesByUserCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteTaskAccessesByUserCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTaskAccessesByUserCommand, bool>
{
    private readonly IValidator<DeleteTaskAccessesByUserCommand> _validator = validator;

    /// <summary>
    /// Processes the command to delete all user-task access entries for a given user.
    /// </summary>
    /// <param name="command">
    /// The command containing the user ID whose access entries should be removed.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access entries were deleted.
    /// </returns>
    public async Task<Result<bool>> HandleAsync(DeleteTaskAccessesByUserCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return await Result<bool>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var exists = await this.UnitOfWork.UserTaskAccesses.ExistsByUserIdAsync(command.UserId, cancellationToken);
        if (!exists)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.InvalidOperation, "There are no tasks shared with you.");
        }

        await this.UnitOfWork.UserTaskAccesses.DeleteAllByUserIdAsync(command.UserId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
