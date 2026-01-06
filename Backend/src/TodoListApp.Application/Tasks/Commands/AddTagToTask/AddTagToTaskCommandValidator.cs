using FluentValidation;

namespace TodoListApp.Application.Tasks.Commands.AddTagToTask;

/// <summary>
/// Validator for <see cref="AddTagToTaskCommand"/>.
/// </summary>
public class AddTagToTaskCommandValidator : AbstractValidator<AddTagToTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddTagToTaskCommandValidator"/> class.
    /// </summary>
    public AddTagToTaskCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        this.RuleFor(x => x.TagId)
            .NotEmpty().WithMessage("Tag ID is required");
    }
}
