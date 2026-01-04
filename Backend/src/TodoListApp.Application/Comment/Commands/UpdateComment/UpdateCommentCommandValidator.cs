using FluentValidation;

namespace TodoListApp.Application.Comment.Commands.UpdateComment;

/// <summary>
/// Validator for <see cref="UpdateCommentCommand"/>.
/// Ensures that the comment text is not empty and does not exceed the maximum allowed length.
/// </summary>
public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCommentCommandValidator"/> class.
    /// </summary>
    public UpdateCommentCommandValidator()
    {
        this.RuleFor(x => x.NewText)
            .NotEmpty().WithMessage("New text cannot be null or empty.")
            .MaximumLength(1000).WithMessage("Comment text cannot exceed 1000 characters.");
    }
}
