using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;

/// <summary>
/// Handles the <see cref="DeleteOverdueTasksCommand"/> and deletes all overdue tasks.
/// in a specified task list for a given user.
/// </summary>
public class DeleteOverdueTasksCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeleteOverdueTasksCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteOverdueTasksCommand, bool>
{
    private readonly IValidator<DeleteOverdueTasksCommand> _validator = validator;

    /// <summary>
    /// Handles the deletion of overdue tasks in the specified task list for a given user.
    /// </summary>
    /// <param name="command">The <see cref="DeleteOverdueTasksCommand"/> containing task list and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(DeleteOverdueTasksCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        await this.UnitOfWork.Tasks.DeleteOverdueTasksAsync(command.UserId, command.TaskListId, DateTime.UtcNow, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return await Result<bool>.SuccessAsync(true);
    }
}
