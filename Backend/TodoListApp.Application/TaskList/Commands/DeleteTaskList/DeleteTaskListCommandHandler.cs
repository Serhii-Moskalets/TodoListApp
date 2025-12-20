using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.TaskList.Commands.DeleteTaskList;

/// <summary>
/// Handles the <see cref="DeleteTaskListCommand"/> by deleting a task list
/// that belongs to a specific user.
/// </summary>
public class DeleteTaskListCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTaskListCommand>
{
    /// <summary>
    /// Handles the command to delete a task list.
    /// </summary>
    /// <param name="command">The command containing the task list ID and user ID.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing <c>true</c> if the task list was successfully deleted;
    /// otherwise, a failure result with an appropriate error code.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteTaskListCommand command, CancellationToken cancellationToken)
    {
        var exists = await this.UnitOfWork.TaskLists.IsTodoListOwnerAsync(command.TaskListId, command.UserId, cancellationToken);

        if (!exists)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, "Task list not found.");
        }

        await this.UnitOfWork.TaskLists.DeleteAsync(command.TaskListId);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
