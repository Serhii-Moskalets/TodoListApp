using FluentValidation;

namespace TodoListApp.Application.TaskList.Commands.UpdateTaskList;

/// <summary>
/// Validates the <see cref="UpdateTaskListCommand"/> to ensure all required properties
/// meet the defined business rules.
/// </summary>
public class UpdateTaskListCommandValidator : AbstractValidator<UpdateTaskListCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskListCommandValidator"/> class.
    /// </summary>
    public UpdateTaskListCommandValidator()
    {
        this.RuleFor(x => x.NewTitle)
            .NotEmpty().WithMessage("New title cannot be null or empty.");
    }
}
