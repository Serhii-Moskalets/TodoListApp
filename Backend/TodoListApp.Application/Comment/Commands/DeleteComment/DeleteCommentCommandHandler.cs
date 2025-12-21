using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Comment.Commands.DeleteComment;

/// <summary>
/// Handles the deletion of an existing comment.
/// </summary>
public class DeleteCommentCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteCommentCommand>
{
    /// <summary>
    /// Handles the specified <see cref="DeleteCommentCommand"/>.
    /// </summary>
    /// <param name="command">
    /// The command containing the ID of the comment to delete and the ID of the user requesting deletion.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating whether the comment was successfully deleted.
    /// Returns a failure result if the comment does not exist or the user is not the owner.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        var exists = await this.UnitOfWork.Comments.IsCommentOwnerAsync(command.CommentId, command.UserId, cancellationToken);

        if (!exists)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, "Comment not found.");
        }

        await this.UnitOfWork.Comments.DeleteAsync(command.CommentId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
