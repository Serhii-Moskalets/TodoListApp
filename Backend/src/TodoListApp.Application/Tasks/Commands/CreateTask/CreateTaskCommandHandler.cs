using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tasks.Commands.CreateTask;

/// <summary>
/// Handles the <see cref="CreateTaskCommand"/> to create a new task.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateTaskCommandHandler"/> class.
/// </remarks>
public class CreateTaskCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<CreateTaskCommand> validator)
    : HandlerBase(unitOfWork), ICommandHandler<CreateTaskCommand, Guid>
{
    private readonly IValidator<CreateTaskCommand> _validator = validator;

    /// <summary>
    /// Handles the creation of a new task based on the provided command.
    /// </summary>
    /// <param name="command">The <see cref="CreateTaskCommand"/> containing task details.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Result{Guid}"/> containing the created task identifier.</returns>
    public async Task<Result<Guid>> HandleAsync(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return await Result<Guid>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var taskList = await this.UnitOfWork.TaskLists
            .GetTaskListByIdForUserAsync(command.Dto.TaskListId, command.UserId, cancellationToken: cancellationToken);

        if (taskList is null)
        {
            return await Result<Guid>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, "Task not found.");
        }

        var task = new TaskEntity(
            command.UserId,
            command.Dto.TaskListId,
            command.Dto.Title!,
            command.Dto.DueDate);

        await this.UnitOfWork.Tasks.AddAsync(task, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(task.Id);
    }
}
