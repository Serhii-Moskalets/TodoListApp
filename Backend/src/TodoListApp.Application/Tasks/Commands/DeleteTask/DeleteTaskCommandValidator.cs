using FluentValidation;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tasks.Commands.DeleteTask;

/// <summary>
/// Validator for <see cref="DeleteTaskCommand"/>.
/// Ensures that the command contains valid data and the user is authorized to delete task..
/// </summary>
public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskCommandValidator"/> class
    /// and sets up validation rules for deleting overdue tasks.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to access the repository for validation.</param>
    public DeleteTaskCommandValidator(IUnitOfWork unitOfWork)
    {
        this.RuleFor(x => x.TaskId)
            .MustAsync(async (command, id, ct) =>
                await unitOfWork.Tasks.ExistsForUserAsync(id, command.UserId, ct))
            .WithMessage("Task not found or does not belong to the user.");
    }
}
