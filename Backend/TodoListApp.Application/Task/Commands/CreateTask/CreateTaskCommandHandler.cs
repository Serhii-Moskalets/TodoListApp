using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Task.Commands.CreateTask;

/// <summary>
/// Handles the <see cref="CreateTaskCommand"/> to create a new task.
/// </summary>
public class CreateTaskCommandHandler : HandlerBase, ICommandHandler<CreateTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to manage repositories and save changes.</param>
    public CreateTaskCommandHandler(IUnitOfWork unitOfWork)
        : base(unitOfWork) { }

    /// <summary>
    /// Handles the creation of a new task based on the provided command.
    /// </summary>
    /// <param name="command">The <see cref="CreateTaskCommand"/> containing task details.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var task = TaskEntity.Create(
            command.Dto.OwnerId,
            command.Dto.TaskListId,
            command.Dto.Title,
            DateTime.UtcNow);

        await this.UnitOfWork.Tasks.AddAsync(task, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
