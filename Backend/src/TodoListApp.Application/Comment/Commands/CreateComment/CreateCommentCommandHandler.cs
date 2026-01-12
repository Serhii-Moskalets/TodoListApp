using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Comment.Commands.CreateComment;

/// <summary>
/// Handles the creation of a new comment for a task.
/// </summary>
/// <remarks>
/// Validates the command, checks task access, creates a comment, and persists it.
/// </remarks>
public class CreateCommentCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskAccessService taskAccessService,
    IValidator<CreateCommentCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<CreateCommentCommand, Guid>
{
    private readonly IValidator<CreateCommentCommand> _validator = validator;
    private readonly ITaskAccessService _taskAccessService = taskAccessService;

    /// <summary>
    /// Handles the specified <see cref="CreateCommentCommand"/>.
    /// </summary>
    /// <param name="command">Data required to create the comment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the ID of the created comment on success,
    /// or an error if validation fails or the user has no access.
    /// </returns>
    public async Task<Result<Guid>> HandleAsync(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return await Result<Guid>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        if (!await this._taskAccessService.HasAccessAsync(command.TaskId, command.UserId, cancellationToken))
        {
            return await Result<Guid>.FailureAsync(ErrorCode.InvalidOperation, "You don't have access to this task.");
        }

        var text = command.Text!;

        var commentEntity = new CommentEntity(command.TaskId, command.UserId, text);

        await this.UnitOfWork.Comments.AddAsync(commentEntity, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(commentEntity.Id);
    }
}
