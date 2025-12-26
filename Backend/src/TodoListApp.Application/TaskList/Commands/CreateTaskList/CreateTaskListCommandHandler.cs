using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.TaskList.Commands.CreateTaskList;

/// <summary>
/// Handles the <see cref="CreateTaskListCommand"/> by creating a new task list
/// for a specific user. If a task list with the same title already exists for
/// the user, a numeric suffix is appended to make the title unique.
/// </summary>
public class CreateTaskListCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<CreateTaskListCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<CreateTaskListCommand>
{
    private readonly IValidator<CreateTaskListCommand> _validator = validator;

    /// <summary>
    /// Processes the command to create a new task list.
    /// </summary>
    /// <param name="command">The command containing the user ID and title for the new task list.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}"/> indicating whether the operation was successful.</returns>
    public async Task<Result<bool>> Handle(CreateTaskListCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var newTitle = command.Title;
        int suffix = 1;

        while (await this.UnitOfWork.TaskLists.ExistsByTitleAsync(newTitle, command.UserId, cancellationToken))
        {
            newTitle = $"{command.Title} ({suffix++})";
        }

        var taskListentity = new TaskListEntity(command.UserId, newTitle);

        await this.UnitOfWork.TaskLists.AddAsync(taskListentity, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
