using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.TaskList.Commands.DeleteTaskList;

/// <summary>
/// Handles the <see cref="DeleteTaskListCommand"/> by deleting a task list
/// that belongs to a specific user.
/// </summary>
public class DeleteTaskListCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteTaskListCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTaskListCommand, bool>
{
    private readonly IValidator<DeleteTaskListCommand> _validator = validator;

    /// <summary>
    /// Handles the command to delete a task list.
    /// </summary>
    /// <param name="command">The command containing the task list ID and user ID.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing <c>true</c> if the task list was successfully deleted;
    /// otherwise, a failure result with an appropriate error code.
    /// </returns>
    public async Task<Result<bool>> HandleAsync(DeleteTaskListCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var taskList = await this.UnitOfWork.TaskLists.GetByIdAsync(command.TaskListId, asNoTracking: false, cancellationToken);
        if (taskList is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task list not found.");
        }

        if (taskList.OwnerId != command.UserId)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "You do not have permission to delete this task list.");
        }

        await this.UnitOfWork.TaskLists.DeleteAsync(taskList, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
