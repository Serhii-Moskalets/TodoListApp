using FluentValidation;

namespace TodoListApp.Application.Comment.Commands.CreateComment;

/// <summary>
/// Validator for <see cref="CreateCommentCommand"/> using FluentValidation.
/// Ensures that the comment text is not empty and does not exceed the maximum allowed length.
/// </summary>
public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentCommandValidator"/> class.
    /// </summary>
    public CreateCommentCommandValidator()
    {
        this.RuleFor(c => c.Text)
            .NotEmpty().WithMessage("Comment text cannot be empty.")
            .MaximumLength(1000).WithMessage("Comment text cannot exceed 1000 characters.");
    }
}
