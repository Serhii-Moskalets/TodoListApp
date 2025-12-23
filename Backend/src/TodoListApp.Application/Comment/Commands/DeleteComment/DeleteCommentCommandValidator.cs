using FluentValidation;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Comment.Commands.DeleteComment;

/// <summary>
/// Validator for <see cref="DeleteCommentCommand"/>.
/// Ensures that the comment exists and belongs to the user attempting the deletion.
/// </summary>
public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCommentCommandValidator"/> class.
    /// </summary>
    /// <param name="unitOfWork">
    /// The unit of work used to access comment data for validation.
    /// </param>
    public DeleteCommentCommandValidator(IUnitOfWork unitOfWork)
    {
        this.RuleFor(x => x.CommentId)
            .MustAsync(async (command, id, ct) =>
                await unitOfWork.Comments.IsCommentOwnerAsync(id, command.UserId, ct))
            .WithMessage("Comment not found or does not belong to the user.");
    }
}
