using FluentValidation;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Task.Commands.DeleteOverdueTasks;

/// <summary>
/// Validator for <see cref="DeleteOverdueTasksCommand"/>.
/// Ensures that the command contains valid data and the user is authorized
/// to delete overdue tasks from the specified task list.
/// </summary>
public class DeleteOverdueTasksCommandValidator : AbstractValidator<DeleteOverdueTasksCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteOverdueTasksCommandValidator"/> class
    /// and sets up validation rules for deleting overdue tasks.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to access the repository for validation.</param>
    public DeleteOverdueTasksCommandValidator(IUnitOfWork unitOfWork)
    {
        this.RuleFor(x => x.TaskListId)
            .MustAsync(async (command, id, ct) =>
                await unitOfWork.TaskLists.IsTaskListOwnerAsync(id, command.UserId, ct))
            .WithMessage("TaskList not found or does not belong to the user.");
    }
}
