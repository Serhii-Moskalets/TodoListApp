using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Task.Commands.UpdateTask;

/// <summary>
/// Handles the <see cref="UpdateTaskCommand"/> to update an existing task.
/// </summary>
public class UpdateTaskCommandHandler : HandlerBase, ICommandHandler<UpdateTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to manage repositories and save changes.</param>
    public UpdateTaskCommandHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    /// <summary>
    /// Handles updating a task for a specific user.
    /// </summary>
    /// <param name="command">The <see cref="UpdateTaskCommand"/> containing update task dto.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var taskDto = command.Dto;
        var exists = await this.UnitOfWork.Tasks.ExistsForUserAsync(taskDto.TaskId, taskDto.OwnerId, cancellationToken);

        if (!exists)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        var taskEntity = await this.UnitOfWork.Tasks.GetByIdAsync(taskDto.TaskId, cancellationToken: cancellationToken);
        if (taskEntity == null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        taskEntity.UpdateDetails(taskDto.Title, taskDto.Description, taskDto.DueDate);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
