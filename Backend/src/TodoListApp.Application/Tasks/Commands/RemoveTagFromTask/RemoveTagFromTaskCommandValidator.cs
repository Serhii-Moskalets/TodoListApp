using FluentValidation;

namespace TodoListApp.Application.Tasks.Commands.RemoveTagFromTask;

/// <summary>
/// Validator for <see cref="RemoveTagFromTaskCommand"/>.
/// </summary>
public class RemoveTagFromTaskCommandValidator : AbstractValidator<RemoveTagFromTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveTagFromTaskCommandValidator"/> class.
    /// </summary>
    public RemoveTagFromTaskCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
