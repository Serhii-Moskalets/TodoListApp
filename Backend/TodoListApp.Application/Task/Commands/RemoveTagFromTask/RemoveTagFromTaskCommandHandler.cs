using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Task.Commands.RemoveTagFromTask;

/// <summary>
/// Handles the <see cref="RemoveTagFromTaskCommand"/> to remove a tag from an existing task.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RemoveTagFromTaskCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work used to manage repositories and save changes.</param>
public class RemoveTagFromTaskCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<RemoveTagFromTaskCommand>
{
    /// <summary>
    /// Handles the removal of a tag for a specific task and user.
    /// </summary>
    /// <param name="command">The <see cref="RemoveTagFromTaskCommand"/> containing the task and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(RemoveTagFromTaskCommand command, CancellationToken cancellationToken)
    {
        var taskEntity = await this.UnitOfWork.Tasks.GetTaskByIdForUserAsync(command.TaskId, command.UserId, cancellationToken);

        if (taskEntity is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        taskEntity.SetTag(null);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
