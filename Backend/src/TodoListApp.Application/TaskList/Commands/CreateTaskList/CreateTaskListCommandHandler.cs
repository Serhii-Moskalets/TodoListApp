using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Services;
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
    ITaskListNameUniquenessService taskListNameUniquenessService,
    IValidator<CreateTaskListCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<CreateTaskListCommand, Guid>
{
    private readonly IValidator<CreateTaskListCommand> _validator = validator;
    private readonly ITaskListNameUniquenessService _taskListNameUniquenessService = taskListNameUniquenessService;

    /// <summary>
    /// Processes the command to create a new task list.
    /// </summary>
    /// <param name="command">The command containing the user ID and title for the new task list.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}"/> indicating whether the operation was successful.</returns>
    public async Task<Result<Guid>> HandleAsync(CreateTaskListCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return await Result<Guid>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: true, cancellationToken);
        if (user is null)
        {
            return await Result<Guid>.FailureAsync(ErrorCode.NotFound, "User not found.");
        }

        string uniqueTitle = await this._taskListNameUniquenessService
            .GetUniqueNameAsync(command.UserId, command.Title!, cancellationToken);

        var taskList = new TaskListEntity(user.Id, uniqueTitle);

        await this.UnitOfWork.TaskLists.AddAsync(taskList, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(taskList.Id);
    }
}
