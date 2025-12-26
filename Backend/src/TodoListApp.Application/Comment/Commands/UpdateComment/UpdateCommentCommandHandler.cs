using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Comment.Commands.UpdateComment;

/// <summary>
/// Handles updating the text of an existing comment.
/// </summary>
public class UpdateCommentCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<UpdateCommentCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<UpdateCommentCommand>
{
    private readonly IValidator<UpdateCommentCommand> _validator = validator;

    /// <summary>
    /// Handles the specified <see cref="UpdateCommentCommand"/>.
    /// </summary>
    /// <param name="command">
    /// The command containing the ID of the comment, the ID of the user, and the new text for the comment.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating whether the comment was successfully updated.
    /// Returns a failure result if the comment does not exist, the user is not the owner.
    /// </returns>
    public async Task<Result<bool>> Handle(UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var comment = await this.UnitOfWork.Comments.GetByIdAsync(command.CommentId, cancellationToken: cancellationToken);
        if (comment is null)
        {
            return await Result<bool>.FailureAsync(
                TinyResult.Enums.ErrorCode.NotFound,
                "Comment not found.");
        }

        comment.Update(command.NewText);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
