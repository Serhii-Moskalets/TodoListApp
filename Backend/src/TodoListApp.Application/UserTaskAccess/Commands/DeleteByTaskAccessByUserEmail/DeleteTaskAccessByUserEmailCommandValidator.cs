using FluentValidation;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteByTaskAccessByUserEmail;

/// <summary>
/// Validates the <see cref="DeleteTaskAccessByUserEmailCommand"/> to ensure that
/// a user-task access entry can be deleted only if it exists
/// and the requesting user is the owner of the task.
/// </summary>
public class DeleteTaskAccessByUserEmailCommandValidator
    : AbstractValidator<DeleteTaskAccessByUserEmailCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessByUserEmailCommandValidator"/> class.
    /// </summary>
    /// <param name="unitOfWork">
    /// The unit of work used to access repositories for validating
    /// task ownership and the existence of user-task access entries.
    /// </param>
    public DeleteTaskAccessByUserEmailCommandValidator(IUnitOfWork unitOfWork)
    {
        this.RuleFor(x => x.Email)
            .MustAsync(async (command, email, ct) =>
                await unitOfWork.UserTaskAccesses.ExistsTaskAccessWithEmail(command.TaskId, email, ct))
            .WithMessage("Task access is not found.");

        this.RuleFor(x => x.TaskId)
            .MustAsync(async (command, taskId, ct) =>
            {
                var taskEntity = await unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken: ct);
                return taskEntity != null && taskEntity.OwnerId == command.UserId;
            })
            .WithMessage("Only task owner can delete access.");
    }
}
