using FluentValidation;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;

/// <summary>
/// Validator for <see cref="DeleteTaskAccessesByTaskCommand"/>.
/// </summary>
public class DeleteTaskAccessesByTaskCommandValidator
    : AbstractValidator<DeleteTaskAccessesByTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessesByTaskCommandValidator"/> class.
    /// </summary>
    public DeleteTaskAccessesByTaskCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
