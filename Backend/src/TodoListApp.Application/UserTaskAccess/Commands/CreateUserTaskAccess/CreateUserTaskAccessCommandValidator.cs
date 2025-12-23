using FluentValidation;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;

/// <summary>
/// Validates the <see cref="CreateUserTaskAccessCommand"/> to ensure that
/// a user can be granted access to a task only if they do not already have access
/// and are not the owner of the task.
/// </summary>
public class CreateUserTaskAccessCommandValidator : AbstractValidator<CreateUserTaskAccessCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserTaskAccessCommandValidator"/> class
    /// and sets up validation rules for creating user-task access.
    /// </summary>
    /// <param name="unitOfWork">
    /// The unit of work used to access repositories for checking existing user-task access
    /// and task ownership.
    /// </param>
    public CreateUserTaskAccessCommandValidator(IUnitOfWork unitOfWork)
    {
        this.RuleFor(x => x.TaskId)
           .MustAsync(async (command, id, ct) =>
               !await unitOfWork.UserTaskAccesses.ExistsAsync(id, command.UserId, ct))
           .WithMessage("User already has access to this task.")
           .MustAsync(async (command, id, ct) =>
               !await unitOfWork.Tasks.IsTaskOwnerAsync(id, command.UserId, ct))
           .WithMessage("User is owner in this task.");
    }
}
