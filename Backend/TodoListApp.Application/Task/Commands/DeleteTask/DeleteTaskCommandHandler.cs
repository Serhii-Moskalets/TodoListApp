using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Task.Commands.DeleteTask;

/// <summary>
/// Handles the <see cref="DeleteTaskCommand"/> to delete an existing task.
/// </summary>
public class DeleteTaskCommandHandler : HandlerBase, ICommandHandler<DeleteTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to manage repositories and save changes.</param>
    public DeleteTaskCommandHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    /// <summary>
    /// Handles the deletion of a task for a specific user.
    /// </summary>
    /// <param name="command">The <see cref="DeleteTaskCommand"/> containing task and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        bool exists = await this.UnitOfWork.Tasks.ExistsForUserAsync(command.TaskId, command.UserId, cancellationToken);

        if (!exists)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        await this.UnitOfWork.Tasks.DeleteAsync(command.TaskId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
