using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Comment.Commands.DeleteComment;

/// <summary>
/// Handles the deletion of an existing comment.
/// </summary>
public class DeleteCommentCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteCommentCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteCommentCommand, bool>
{
    private readonly IValidator<DeleteCommentCommand> _validator = validator;

    /// <summary>
    /// Deletes the comment identified by the command if allowed.
    /// </summary>
    /// <param name="command">Comment ID and user ID requesting deletion.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success if deleted; otherwise, a failure result.</returns>
    public async Task<Result<bool>> HandleAsync(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return await Result<bool>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var comment = await this.UnitOfWork.Comments.GetByIdAsync(command.CommentId, asNoTracking: true, cancellationToken);
        if (comment is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Comment not found.");
        }

        var isCommentOwner = command.UserId == comment.UserId;
        var isTaskOwner = await this.UnitOfWork.Tasks.IsTaskOwnerAsync(comment.TaskId, command.UserId, cancellationToken);

        if (!isCommentOwner && !isTaskOwner)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "You don't have permission to delete this comment.");
        }

        await this.UnitOfWork.Comments.DeleteAsync(comment, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
