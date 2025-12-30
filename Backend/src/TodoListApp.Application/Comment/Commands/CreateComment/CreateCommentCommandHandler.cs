using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;

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
    : HandlerBase(unitOfWork), ICommandHandler<CreateCommentCommand, Guid>
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
    public async Task<Result<Guid>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return await Result<Guid>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        ArgumentNullException.ThrowIfNullOrWhiteSpace(command.Text);
        var commentEntity = new CommentEntity(command.TaskId, command.UserId, command.Text);

        await this.UnitOfWork.Comments.AddAsync(commentEntity, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(commentEntity.Id);
    }
}
