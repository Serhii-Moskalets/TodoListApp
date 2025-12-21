using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteByTaskAccessByUserEmail;

/// <summary>
/// Handles the deletion of a user-task access entry based on the task ID and the user's email.
/// </summary>
public class DeleteTaskAccessByUserEmailCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteTaskAccessByUserEmailCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTaskAccessByUserEmailCommand>
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

        await this.UnitOfWork.UserTaskAccesses.DeleteByUserEmailAsync(command.TaskId, command.Email, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
