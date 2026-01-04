using FluentValidation;

namespace TodoListApp.Application.Comment.Commands.CreateComment;

/// <summary>
/// Validator for <see cref="CreateCommentCommand"/>.
/// </summary>
public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentCommandValidator"/> class.
    /// </summary>
    public CreateCommentCommandValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        this.RuleFor(c => c.Text)
            .NotEmpty().WithMessage("Comment text cannot be null or empty.")
            .MaximumLength(1000).WithMessage("Comment text cannot exceed 1000 characters.");
    }
}
