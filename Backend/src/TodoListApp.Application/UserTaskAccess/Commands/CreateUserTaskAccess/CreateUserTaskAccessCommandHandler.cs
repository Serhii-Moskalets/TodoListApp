using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;

/// <summary>
/// Handles the creation of a user-task access relationship.
/// </summary>
public class CreateUserTaskAccessCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<CreateUserTaskAccessCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<CreateUserTaskAccessCommand, bool>
{
    private readonly IValidator<CreateUserTaskAccessCommand> _validator = validator;

    /// <summary>
    /// Handles the <see cref="CreateUserTaskAccessCommand"/> to grant a user access to a task.
    /// </summary>
    /// <param name="command">The command containing the task ID and user email.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was created,
    /// or failure if the user does not exist, is the task owner, or already has access.
    /// </returns>
    public async Task<Result<bool>> Handle(CreateUserTaskAccessCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var email = command.Email!;

        var sharedUser = await this.UnitOfWork.Users.GetByEmailAsync(email, cancellationToken);

        var accessValidation = await this.ValidateAsync(command.TaskId, command.OwnerId, sharedUser, cancellationToken);
        if (!accessValidation.IsSuccess)
        {
            return accessValidation;
        }

        var access = new UserTaskAccessEntity(command.TaskId, sharedUser!.Id);

        await this.UnitOfWork.UserTaskAccesses.AddAsync(access, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }

    /// <summary>
    /// Validates the user and task access rules before creating a new access entry.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="ownerId">The current owner ID performing the action.</param>
    /// <param name="sharedUser">The user to grant access to.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if all validation rules pass,
    /// or failure if any rule is violated.
    /// </returns>
    private async Task<Result<bool>> ValidateAsync(
        Guid taskId,
        Guid ownerId,
        UserEntity? sharedUser,
        CancellationToken cancellationToken)
    {
        if (sharedUser is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "User not found.");
        }

        if (!await this.IsOwnerAsync(taskId, ownerId, cancellationToken))
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, "Current user haven't access for this task.");
        }

        if (await this.IsOwnerAsync(taskId, sharedUser.Id, cancellationToken))
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, "Task cannot be shared with its owner.");
        }

        if (await this.HasAccessAsync(taskId, sharedUser.Id, cancellationToken))
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "Task already shared with this user.");
        }

        return await Result<bool>.SuccessAsync(true);
    }

    /// <summary>
    /// Checks whether the specified user is the owner of the task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns><c>true</c> if the user is the owner; otherwise, <c>false</c>.</returns>
    private async Task<bool> IsOwnerAsync(Guid taskId, Guid userId,  CancellationToken cancellationToken = default)
        => await this.UnitOfWork.Tasks.IsTaskOwnerAsync(taskId, userId, cancellationToken);

    /// <summary>
    /// Checks whether the specified user already has access to the task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns><c>true</c> if the user already has access; otherwise, <c>false</c>.</returns>
    private async Task<bool> HasAccessAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
       => await this.UnitOfWork.UserTaskAccesses.ExistsAsync(taskId, userId, cancellationToken);
}
