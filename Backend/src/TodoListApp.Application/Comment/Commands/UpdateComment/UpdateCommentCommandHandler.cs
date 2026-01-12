using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Comment.Commands.UpdateComment;

/// <summary>
/// Handles updating the text of an existing comment.
/// </summary>
public class UpdateCommentCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<UpdateCommentCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<UpdateCommentCommand, bool>
{
    private readonly IValidator<UpdateCommentCommand> _validator = validator;

    /// <summary>
    /// Updates the comment if the user is the owner and validation passes.
    /// </summary>
    /// <param name="command">The comment ID, user ID, and new text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success if updated; otherwise, a failure result.</returns>
    public async Task<Result<bool>> HandleAsync(UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var comment = await this.UnitOfWork.Comments.GetByIdAsync(command.CommentId, false, cancellationToken);
        if (comment is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Comment not found.");
        }

        if (comment.UserId != command.UserId)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "You don't have permission to update this comment.");
        }

        var newText = command.NewText!;
        comment.Update(newText);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
