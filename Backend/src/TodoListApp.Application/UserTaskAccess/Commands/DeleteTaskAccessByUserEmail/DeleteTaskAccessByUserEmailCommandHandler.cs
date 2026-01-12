using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

/// <summary>
/// Handles the deletion of a user-task access entry based on the task ID and the user's email.
/// </summary>
public class DeleteTaskAccessByUserEmailCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteTaskAccessByUserEmailCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTaskAccessByUserEmailCommand, bool>
{
    private readonly IValidator<DeleteTaskAccessByUserEmailCommand> _validator = validator;

    /// <summary>
    /// Handles the <see cref="DeleteTaskAccessByUserEmailCommand"/> by checking if the access exists,
    /// deleting it if present, and returning the operation result.
    /// </summary>
    /// <param name="command">The command containing the task ID and user email.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was deleted,
    /// or failure if the access was not found.
    /// </returns>
    public async Task<Result<bool>> HandleAsync(DeleteTaskAccessByUserEmailCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return await Result<bool>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var email = command.Email!.Trim().ToLowerInvariant();

        var hasAccess = await this.UnitOfWork.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, cancellationToken);
        if (!hasAccess)
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, "User doesn't have access to this task.");
        }

        var sharedUser = await this.UnitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (sharedUser is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "Opperation error.");
        }

        var deleted = await this.UnitOfWork.UserTaskAccesses.DeleteByIdAsync(command.TaskId, sharedUser!.Id, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (deleted <= 0)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "Access doesn't deleted.");
        }

        return await Result<bool>.SuccessAsync(true);
    }
}
