using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Task.Commands.ChangeTaskStatus;

/// <summary>
/// Handles the <see cref="ChangeTaskStatusCommand"/> by changing the status of a task
/// that belongs to a specific user.
/// </summary>
public class ChangeTaskStatusCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<ChangeTaskStatusCommand>
{
    /// <summary>
    /// Processes the command to change the status of an existing task.
    /// </summary>
    /// <param name="command">
    /// The command containing the task identifier, user identifier,
    /// and the new status to be applied.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating whether the operation
    /// completed successfully.
    /// </returns>
    public async Task<Result<bool>> Handle(ChangeTaskStatusCommand command, CancellationToken cancellationToken)
    {
        var entity = await this.UnitOfWork.Tasks.GetTaskByIdForUserAsync(command.TaskId, command.UserId, cancellationToken);

        if (entity is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        try
        {
            entity.ChangeStatus(command.Status);
            await this.UnitOfWork.SaveChangesAsync(cancellationToken);
            return await Result<bool>.SuccessAsync(true);
        }
        catch (DomainException ex)
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, ex.Message);
        }
    }
}
