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

        var uniqueName = await this.GetUniqueName(command.Name!, command.UserId, cancellationToken);

        var tagEntity = new TagEntity(uniqueName, command.UserId);
        await this.UnitOfWork.Tags.AddAsync(tagEntity, cancellationToken);

        var addTagCommand = new AddTagToTaskCommand(command.TaskId, command.UserId, tagEntity.Id);
        var tagResult = await this._addTagToTaskHandler.HandleAsync(addTagCommand, cancellationToken);

        if (!tagResult.IsSuccess)
        {
            await this.UnitOfWork.Tags.DeleteAsync(tagEntity, cancellationToken);

            return await Result<Guid>.FailureAsync(tagResult.Error!.Code, tagResult.Error.Message);
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(tagEntity.Id);
    }

    /// <summary>
    /// Generates a unique tag name for the specified user by appending
    /// a numeric suffix if a tag with the same name already exists.
    /// </summary>
    /// <param name="name">The desired tag name.</param>
    /// <param name="userId">The identifier of the tag owner.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while checking for existing tags.
    /// </param>
    /// <returns>
    /// A unique tag name that does not conflict with existing tags of the user.
    /// </returns>
    private async Task<string> GetUniqueName(string name, Guid userId, CancellationToken cancellationToken)
    {
        var newName = name;
        var siffix = 1;

        while (await this.UnitOfWork.Tags.ExistsByNameAsync(newName!, userId, cancellationToken))
        {
            newName = $"{name} ({siffix++})";
        }

        return newName;
    }
}
