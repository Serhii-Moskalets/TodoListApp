using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Comment.Commands.CreateComment;

/// <summary>
/// Handles the creation of a new comment for a task.
/// </summary>
/// <remarks>
/// This handler creates a <see cref="CommentEntity"/>,
/// persists it using the unit of work,
/// and returns a success result if the operation completes successfully.
/// </remarks>
public class CreateCommentCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<CreateCommentCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<CreateCommentCommand>
{
    private readonly IValidator<CreateCommentCommand> _validator = validator;

    /// <summary>
    /// Handles the specified <see cref="CreateCommentCommand"/>.
    /// </summary>
    /// <param name="command">
    /// The command containing the data required to create a new comment.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating whether the comment was created successfully.
    /// </returns>
    public async Task<Result<bool>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var commentEntity = new CommentEntity(command.TaskId, command.UserId, command.Text);

        await this.UnitOfWork.Comments.AddAsync(commentEntity, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
