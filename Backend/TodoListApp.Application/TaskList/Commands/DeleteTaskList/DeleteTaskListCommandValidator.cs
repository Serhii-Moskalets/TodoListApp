using FluentValidation;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.TaskList.Commands.DeleteTaskList;

/// <summary>
/// Validates the <see cref="DeleteTaskListCommand"/> to ensure the task list exists
/// and belongs to the requesting user before deletion.
/// </summary>
public class DeleteTaskListCommandValidator : AbstractValidator<DeleteTaskListCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskListCommandValidator"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to access the data store.</param>
    public DeleteTaskListCommandValidator(IUnitOfWork unitOfWork)
    {
        this.RuleFor(x => x.TaskListId)
            .MustAsync(async (command, id, ct) => 
                await unitOfWork.TaskLists.IsTodoListOwnerAsync(id, command.UserId, ct))
            .WithMessage("Task list not found or does not belong to the user.");
    }
}
