using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.TaskList.Commands.UpdateTaskList;

/// <summary>
/// Handles the <see cref="UpdateTaskListCommand"/> by updating the title of a task list
/// that belongs to a specific user.
/// </summary>
public class UpdateTaskListCommandHandler(
    IUnitOfWork unitOfWork,
    ITaskListNameUniquenessService taskListNameUniquenessService,
    IValidator<UpdateTaskListCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<UpdateTaskListCommand, bool>
{
    private readonly IValidator<UpdateTaskListCommand> _validator = validator;
    private readonly ITaskListNameUniquenessService _taskListNameUniquenessService = taskListNameUniquenessService;

    /// <summary>
    /// Handles the command to update the title of a task list.
    /// </summary>
    /// <param name="command">The command containing the task list ID, user ID, and new title.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing <c>true</c> if the task list title was successfully updated;
    /// otherwise, a failure result with an appropriate error code.
    /// </returns>
    public async Task<Result<bool>> HandleAsync(UpdateTaskListCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var taskList = await this.UnitOfWork.TaskLists.GetByIdAsync(command.TaskListId, false, cancellationToken);

        if (taskList is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task list not found.");
        }

        if (taskList.Title == command.NewTitle)
        {
            return await Result<bool>.SuccessAsync(true);
        }

        if (taskList.OwnerId != command.UserId)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "You do not have permission to update this task list.");
        }

        string uniqueTitle = await this._taskListNameUniquenessService
            .GetUniqueNameAsync(command.UserId, command.NewTitle!, cancellationToken);

        taskList.UpdateTitle(uniqueTitle);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
