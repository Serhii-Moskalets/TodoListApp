using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Task.Commands.DeleteOverdueTasks;

/// <summary>
/// Handles the <see cref="DeleteOverdueTasksCommand"/> and deletes all overdue tasks.
/// in a specified task list for a given user.
/// </summary>
public class DeleteOverdueTasksCommandHandler : ICommandHandler<DeleteOverdueTasksCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteOverdueTasksCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to manage repositories and save changes.</param>
    public DeleteOverdueTasksCommandHandler(IUnitOfWork unitOfWork)
    {
        this._unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the deletion of overdue tasks in the specified task list for a given user.
    /// </summary>
    /// <param name="command">The <see cref="DeleteOverdueTasksCommand"/> containing task list and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(DeleteOverdueTasksCommand command, CancellationToken cancellationToken)
    {
        await this._unitOfWork.Tasks.DeleteOverdueTasksAsync(command.UserId, command.TaskListId, DateTime.UtcNow, cancellationToken);
        await this._unitOfWork.SaveChangesAsync(cancellationToken);
        return await Result<bool>.SuccessAsync(true);
    }
}
