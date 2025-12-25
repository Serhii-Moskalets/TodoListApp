using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.UnitOfWork;

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
    : HandlerBase(unitOfWork), ICommandHandler<CreateTaskCommand>
{
    private readonly IValidator<CreateTaskCommand> _validator = validator;

    /// <summary>
    /// Handles the creation of a new task based on the provided command.
    /// </summary>
    /// <param name="command">The <see cref="CreateTaskCommand"/> containing task details.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, command);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var task = new TaskEntity(
            command.Dto.OwnerId,
            command.Dto.TaskListId,
            command.Dto.Title,
            command.Dto.DueDate);

        await this.UnitOfWork.Tasks.AddAsync(task, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
