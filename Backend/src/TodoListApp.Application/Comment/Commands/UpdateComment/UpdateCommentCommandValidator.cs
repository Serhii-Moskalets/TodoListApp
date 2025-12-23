using FluentValidation;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Comment.Commands.UpdateComment;

/// <summary>
/// Validator for <see cref="UpdateCommentCommand"/>.
/// Ensures that the comment exists and belongs to the user attempting the deletion.
/// </summary>
public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCommentCommandValidator"/> class.
    /// </summary>
    /// <param name="unitOfWork">
    /// The unit of work used to access comment data for validation.
    /// </param>
    public UpdateCommentCommandValidator(IUnitOfWork unitOfWork)
    {
        this.RuleFor(x => x.CommentId)
            .MustAsync(async (command, id, ct) =>
                await unitOfWork.Comments.IsCommentOwnerAsync(id, command.UserId, ct))
            .WithMessage("Comment not found or does not belong to the user.");

        this.RuleFor(x => x.NewText)
            .NotEmpty().NotNull().WithMessage("New text cannot be null or empty.")
            .MaximumLength(1000).WithMessage("Comment text cannot exceed 1000 characters.");
    }
}
