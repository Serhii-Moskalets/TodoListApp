using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tag.Commands.DeleteTag;

/// <summary>
/// Handles the <see cref="DeleteTagCommand"/> by deleting a tag that belongs to a specific user.
/// </summary>
public class DeleteTagCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteTagCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTagCommand>
{
    private readonly IValidator<DeleteTagCommand> _validator = validator;

    /// <summary>
    /// Processes the command to delete a user's tag.
    /// </summary>
    /// <param name="command">
    /// The command containing the ID of the tag to delete and the user ID of the owner.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the tag was deleted,
    /// or failure if the tag was not found or the user is not the owner.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        await this.UnitOfWork.Tags.DeleteAsync(command.TagId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
