using FluentValidation;

namespace TodoListApp.Application.Tasks.Commands.ChangeTaskStatus;

/// <summary>
/// Validator for <see cref="ChangeTaskStatusCommand"/>.
/// </summary>
public class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeTaskStatusCommandValidator"/> class.
    /// </summary>
    public ChangeTaskStatusCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid task status.");
    }
}
