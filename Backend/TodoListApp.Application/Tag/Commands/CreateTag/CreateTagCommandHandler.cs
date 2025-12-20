using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tag.Commands.CreateTag;

/// <summary>
/// Handles the <see cref="CreateTagCommand"/> by creating a new tag
/// for a specific user. If a tag with the same name already exists for
/// the user, a numeric suffix is appended to make the name unique.
/// </summary>
public class CreateTagCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<CreateTagCommand>
{
    /// <summary>
    /// Processes the command to create a new tag.
    /// </summary>
    /// <param name="command">The command containing the user ID and name for the new tag.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}"/> indicating whether the operation was successful.</returns>
    public async Task<Result<bool>> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        var newName = command.Name;
        var siffix = 1;

        while (await this.UnitOfWork.Tags.ExistsByNameAsync(newName, command.UserId, cancellationToken))
        {
            newName = $"{command.Name} ({siffix++})";
        }

        var tagEntity = new TagEntity(newName, command.UserId);

        await this.UnitOfWork.Tags.AddAsync(tagEntity, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
