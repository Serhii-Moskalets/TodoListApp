using FluentValidation;

namespace TodoListApp.Application.Tasks.Commands.DeleteTask;

/// <summary>
/// Validator for <see cref="DeleteTaskCommand"/>.
/// </summary>
public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskCommandValidator"/> class.
    /// </summary>
    public DeleteTaskCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Owner ID is required.");
    }
}
