using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

/// <summary>
/// Handles the <see cref="DeleteTaskAccessByIdCommand"/> to remove a user-task access entry.
/// </summary>
public class DeleteTaskAccessByIdCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteTaskAccessByIdCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTaskAccessByIdCommand, bool>
{
    private readonly IValidator<DeleteTaskAccessByIdCommand> _validator = validator;

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
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        await this.UnitOfWork.UserTaskAccesses.DeleteByTaskAndUserIdAsync(command.TaskId, command.UserId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
