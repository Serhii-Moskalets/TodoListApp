using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.DeleteTask;

/// <summary>
/// Handles the <see cref="DeleteTaskCommand"/> to delete an existing task.
/// </summary>
public class DeleteTaskCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteTaskCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTaskCommand, bool>
{
    private readonly IValidator<DeleteTaskCommand> _validator = validator;

    /// <summary>
    /// Handles the deletion of a task for a specific user.
    /// </summary>
    /// <param name="command">The <see cref="DeleteTaskCommand"/> containing task and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> HandleAsync(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(command.TaskId, command.UserId, asNoTracking: false, cancellationToken);
        if (task is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        await this.UnitOfWork.Tasks.DeleteAsync(task, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
