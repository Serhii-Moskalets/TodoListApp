using FluentValidation;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

/// <summary>
/// Validates the <see cref="DeleteTaskAccessByIdCommand"/> to ensure that
/// the specified user-task access exists before attempting deletion.
/// </summary>
public class DeleteTaskAccessByIdCommandValidator : AbstractValidator<DeleteTaskAccessByIdCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessByIdCommandValidator"/> class
    /// and defines validation rules for deleting a user-task access entry.
    /// </summary>
    /// <param name="unitOfWork">
    /// The unit of work used to access repositories for validating
    /// the existence of the user-task access entry.
    /// </param>
    public DeleteTaskAccessByIdCommandValidator(IUnitOfWork unitOfWork)
    {
        this.RuleFor(x => x.TaskId)
            .MustAsync(async (command, id, ct) =>
                await unitOfWork.UserTaskAccesses.ExistsAsync(id, command.UserId, ct))
            .WithMessage("User task access not found.");
    }
}
