using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;
using TodoListApp.Domain.Entities;

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
    public async Task<Result<bool>> Handle(DeleteTaskAccessByUserEmailCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var email = command.Email!;

        var sharedUser = await this.UnitOfWork.Users.GetByEmailAsync(email, cancellationToken);

        var accessValidation = await this.ValidationAsync(command.TaskId, command.OwnerId, sharedUser, cancellationToken);
        if (!accessValidation.IsSuccess)
        {
            return accessValidation;
        }

        var deleted = await this.UnitOfWork.UserTaskAccesses.DeleteByIdAsync(command.TaskId, sharedUser!.Id, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (deleted <= 0)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "Access doesn't deleted.");
        }

        return await Result<bool>.SuccessAsync(true);
    }

    private async Task<Result<bool>> ValidationAsync(
        Guid taskId,
        Guid ownerId,
        UserEntity? sharedUser,
        CancellationToken cancellationToken)
    {
        if (sharedUser is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "User not found.");
        }

        if (!await this.IsOwner(taskId, ownerId, cancellationToken))
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "Only task owner can delete access.");
        }

        if (!await this.UserHasAccess(taskId, sharedUser.Id, cancellationToken))
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task access is not found.");
        }

        return await Result<bool>.SuccessAsync(true);
    }

    /// <summary>
    /// Checks whether the specified user already has access to the task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns><c>true</c> if the user does not have access; otherwise, <c>false</c>.</returns>
    private async Task<bool> UserHasAccess(Guid taskId, Guid userId, CancellationToken cancellationToken)
        => await this.UnitOfWork.UserTaskAccesses.ExistsAsync(taskId, userId, cancellationToken);

    /// <summary>
    /// Checks whether the specified user is the owner of the task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns><c>true</c> if the user is not the owner; otherwise, <c>false</c>.</returns>
    private async Task<bool> IsOwner(Guid taskId, Guid userId, CancellationToken cancellationToken)
        => await this.UnitOfWork.Tasks.IsTaskOwnerAsync(taskId, userId, cancellationToken);
}
