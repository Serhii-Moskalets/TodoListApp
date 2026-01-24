using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.UpdateTask;

/// <summary>
/// Handles the <see cref="UpdateTaskCommand"/> to update an existing task.
/// </summary>
public class UpdateTaskCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<UpdateTaskCommand, Result<bool>>
{
    /// <summary>
    /// Handles updating a task for a specific user.
    /// </summary>
    /// <param name="command">The <see cref="UpdateTaskCommand"/> containing update task dto.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(command.Dto.TaskId, command.UserId, false, cancellationToken: cancellationToken);
        if (task == null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        var updateDto = command.Dto;

        if (updateDto.Title == task.Title &&
            updateDto.Description == task.Description &&
            updateDto.DueDate == task.DueDate)
        {
            return await Result<bool>.SuccessAsync(true);
        }

        task.UpdateDetails(command.Dto.Title, command.Dto.Description, command.Dto.DueDate);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
