using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.AddTagToTask;

/// <summary>
/// Handles the <see cref="AddTagToTaskCommand"/> by associating a tag with a specific task for a user.
/// </summary>
public class AddTagToTaskCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<AddTagToTaskCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<AddTagToTaskCommand, bool>
{
    private readonly IValidator<AddTagToTaskCommand> _validator = validator;

    /// <summary>
    /// Handles the <see cref="AddTagToTaskCommand"/>.
    /// </summary>
    /// <param name="command">The command containing TaskId, UserId, and TagId.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the tag was added,
    /// or failure if the task was not found.
    /// </returns>
    public async Task<Result<bool>> HandleAsync(AddTagToTaskCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return await Result<bool>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(command.TaskId, command.UserId, asNoTracking: false, cancellationToken);

        if (task is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        if (task.TagId == command.TagId)
        {
            return await Result<bool>.SuccessAsync(true);
        }

        var tag = await this.UnitOfWork.Tags.GetByIdAsync(command.TagId, true, cancellationToken);
        if (tag is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Tag not found.");
        }

        if (tag.UserId != command.UserId)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "You do not own this tag.");
        }

        task.SetTag(command.TagId);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
