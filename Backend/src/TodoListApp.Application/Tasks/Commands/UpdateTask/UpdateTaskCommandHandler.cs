using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.UpdateTask;

/// <summary>
/// Handles the <see cref="UpdateTaskCommand"/> to update an existing task.
/// </summary>
public class UpdateTaskCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<UpdateTaskCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<UpdateTaskCommand, bool>
{
    private readonly IValidator<UpdateTaskCommand> _validator = validator;

    /// <summary>
    /// Handles updating a task for a specific user.
    /// </summary>
    /// <param name="command">The <see cref="UpdateTaskCommand"/> containing update task dto.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var taskDto = command.Dto;

        var taskEntity = await this.UnitOfWork.Tasks.GetByIdAsync(taskDto.TaskId, false, cancellationToken: cancellationToken);
        if (taskEntity == null || taskEntity.OwnerId != taskDto.OwnerId)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        taskEntity.UpdateDetails(taskDto.Title!, taskDto.Description, taskDto.DueDate);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
