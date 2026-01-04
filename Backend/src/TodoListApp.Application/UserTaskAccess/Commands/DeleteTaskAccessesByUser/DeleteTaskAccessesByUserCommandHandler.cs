using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;

/// <summary>
/// Handles the <see cref="DeleteTaskAccessesByUserCommand"/> to remove all user-task access entries for a specific user.
/// </summary>
public class DeleteTaskAccessesByUserCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTaskAccessesByUserCommand, bool>
{
    /// <summary>
    /// Processes the command to delete all user-task access entries for a given user.
    /// </summary>
    /// <param name="command">
    /// The command containing the user ID whose access entries should be removed.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access entries were deleted.
    /// </returns>
    public async Task<Result<bool>> HandleAsync(DeleteTaskAccessesByUserCommand command, CancellationToken cancellationToken)
    {
        await this.UnitOfWork.UserTaskAccesses.DeleteAllByUserIdAsync(command.UserId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
