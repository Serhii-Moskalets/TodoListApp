using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.Services;
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
    IUniqueNameService uniqueNameService,
    ISender mediator)
    : HandlerBase(unitOfWork), IRequestHandler<CreateTagCommand, Result<Guid>>
{
    /// <summary>
    /// Processes the command to create a new tag.
    /// </summary>
    /// <param name="command">The command containing the user ID and name for the new tag.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}"/> indicating whether the operation was successful.</returns>
    public async Task<Result<Guid>> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        var uniqueName = await uniqueNameService.GetUniqueNameAsync(
            command.Name!,
            (name, ct) => this.UnitOfWork.Tags.ExistsByNameAsync(name, command.UserId, ct),
            cancellationToken);

        var tagEntity = new TagEntity(uniqueName, command.UserId);
        await this.UnitOfWork.Tags.AddAsync(tagEntity, cancellationToken);

        var addTagCommand = new AddTagToTaskCommand(command.TaskId, command.UserId, tagEntity.Id);
        var tagResult = await mediator.Send(addTagCommand, cancellationToken);

        if (!tagResult.IsSuccess)
        {
            await this.UnitOfWork.Tags.DeleteAsync(tagEntity, cancellationToken);
            return await Result<Guid>.FailureAsync(tagResult.Error!.Code, tagResult.Error.Message);
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(tagEntity.Id);
    }
}
