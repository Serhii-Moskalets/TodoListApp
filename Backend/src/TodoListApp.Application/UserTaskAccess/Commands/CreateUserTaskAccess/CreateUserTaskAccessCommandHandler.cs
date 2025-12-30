using FluentValidation;
using TinyResult;
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
    /// <param name="command">The command containing the task ID and user ID.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was created,
    /// or failure if the user already has access or is the task owner.
    /// </returns>
    public async Task<Result<bool>> Handle(CreateUserTaskAccessCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var userTaskAccess = new UserTaskAccessEntity(command.TaskId, command.UserId);

        await this.UnitOfWork.UserTaskAccesses.AddAsync(userTaskAccess, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
