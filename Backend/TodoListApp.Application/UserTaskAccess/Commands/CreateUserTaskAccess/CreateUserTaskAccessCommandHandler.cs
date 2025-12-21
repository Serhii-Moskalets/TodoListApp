using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;

/// <summary>
/// Handles the creation of a user-task access relationship.
/// </summary>
public class CreateUserTaskAccessCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<CreateUserTaskAccessCommand>
{
    /// <summary>
    /// Handles the <see cref="CreateUserTaskAccessCommand"/> to grant a user access to a task.
    /// </summary>
    /// <param name="command">The command containing the task ID and user ID.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was created,
    /// or failure if the user already has access or is the task owner.
    /// </returns>
    public async Task<Result<bool>> Handle(CreateUserTaskAccessCommand command, CancellationToken cancellationToken)
    {
        var exists = await this.UnitOfWork.UserTaskAccesses.ExistsAsync(command.TaskId, command.UserId, cancellationToken);

        if (exists)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.InvalidOperation, "User already has access to this task.");
        }

        if (await this.UnitOfWork.Tasks.IsTaskOwnerAsync(command.TaskId, command.UserId, cancellationToken))
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.InvalidOperation, "User is owner in this task.");
        }

        var userTaskAccess = new UserTaskAccessEntity(command.TaskId, command.UserId);

        await this.UnitOfWork.UserTaskAccesses.AddAsync(userTaskAccess, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
