using FluentValidation;
using TinyResult;
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
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        await this.UnitOfWork.Comments.DeleteAsync(command.CommentId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
