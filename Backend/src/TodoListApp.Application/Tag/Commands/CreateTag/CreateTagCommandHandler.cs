using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Tasks.Commands.AddTagToTask;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tag.Commands.CreateTag;

/// <summary>
/// Handles the <see cref="CreateTagCommand"/> by creating a new tag
/// for a specific user and optionally associating it with a task.
/// </summary>
public class CreateTagCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<CreateTagCommand> validator,
    ICommandHandler<AddTagToTaskCommand, bool> addTagToTaskHandler)
    : HandlerBase(unitOfWork), ICommandHandler<CreateTagCommand, Guid>
{
    private readonly IValidator<CreateTagCommand> _validator = validator;
    private readonly ICommandHandler<AddTagToTaskCommand, bool> _addTagToTaskHandler = addTagToTaskHandler;

    /// <summary>
    /// Processes the command to create a new tag.
    /// </summary>
    /// <param name="command">The command containing the user ID and name for the new tag.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}"/> indicating whether the operation was successful.</returns>
    public async Task<Result<Guid>> HandleAsync(CreateTagCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return await Result<Guid>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var newName = command.Name;
        var siffix = 1;

        while (await this.UnitOfWork.Tags.ExistsByNameAsync(newName!, command.UserId, cancellationToken))
        {
            newName = $"{command.Name} ({siffix++})";
        }

        var tagEntity = new TagEntity(newName!, command.UserId);
        await this.UnitOfWork.Tags.AddAsync(tagEntity, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (command.TaskId.HasValue)
        {
            var addTagCommand = new AddTagToTaskCommand(command.TaskId.Value, command.UserId, tagEntity.Id);
            var tagResult = await this._addTagToTaskHandler.HandleAsync(addTagCommand, cancellationToken);

            if (!tagResult.IsSuccess)
            {
                await this.UnitOfWork.Tags.DeleteAsync(tagEntity.Id, cancellationToken);
                await this.UnitOfWork.SaveChangesAsync(cancellationToken);

                return await Result<Guid>.FailureAsync(tagResult.Error!.Code, tagResult.Error.Message);
            }
        }

        return await Result<Guid>.SuccessAsync(tagEntity.Id);
    }
}
