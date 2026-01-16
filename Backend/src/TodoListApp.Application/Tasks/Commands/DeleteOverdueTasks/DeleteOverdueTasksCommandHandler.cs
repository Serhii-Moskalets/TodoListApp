using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;

/// <summary>
/// Handles the <see cref="DeleteOverdueTasksCommand"/> and deletes all overdue tasks.
/// in a specified task list for a given user.
/// </summary>
public class DeleteOverdueTasksCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteOverdueTasksCommand, Result<bool>>
{
    /// <summary>
    /// Handles the deletion of overdue tasks in the specified task list for a given user.
    /// </summary>
    /// <param name="command">The <see cref="DeleteOverdueTasksCommand"/> containing task list and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(DeleteOverdueTasksCommand command, CancellationToken cancellationToken)
    {
        var taskList = await this.UnitOfWork.TaskLists
            .GetTaskListByIdForUserAsync(command.TaskListId, command.UserId, asNoTracking: false, cancellationToken);

        if (taskList is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task list not found.");
        }

        taskList.DeleteOverdueTasks(DateTime.UtcNow);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
