using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;

/// <summary>
/// Handles the creation of a user-task access relationship.
/// </summary>
public class CreateUserTaskAccessCommandHandler(
    IUnitOfWork unitOfWork,
    IUserTaskAccessService userTaskAccessService,
    IValidator<CreateUserTaskAccessCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<CreateUserTaskAccessCommand, bool>
{
    private readonly IValidator<CreateUserTaskAccessCommand> _validator = validator;
    private readonly IUserTaskAccessService _userTaskAccessService = userTaskAccessService;

    /// <summary>
    /// Handles the <see cref="CreateUserTaskAccessCommand"/> to grant a user access to a task.
    /// </summary>
    /// <param name="command">The command containing the task ID and user email.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was created,
    /// or failure if the user does not exist, is the task owner, or already has access.
    /// </returns>
    public async Task<Result<bool>> HandleAsync(CreateUserTaskAccessCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return await Result<bool>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var email = command.Email!.Trim().ToLowerInvariant();

        var sharedUser = await this.UnitOfWork.Users.GetByEmailAsync(email, cancellationToken);

        var accessValidation = await this._userTaskAccessService
            .CanGrantAccessAsync(command.TaskId, command.OwnerId, sharedUser, cancellationToken);
        if (!accessValidation.IsSuccess)
        {
            return accessValidation;
        }

        var access = new UserTaskAccessEntity(command.TaskId, sharedUser!.Id);

        await this.UnitOfWork.UserTaskAccesses.AddAsync(access, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
