using FluentValidation;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

/// <summary>
/// Validator for <see cref="DeleteTaskAccessByIdCommand"/>.
/// </summary>
public class DeleteTaskAccessByIdCommandValidator : AbstractValidator<DeleteTaskAccessByIdCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessByIdCommandValidator"/> class.
    /// </summary>
    public DeleteTaskAccessByIdCommandValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
