using FluentValidation;

namespace TodoListApp.Application.Task.Commands.UpdateTask;

/// <summary>
/// Validator for <see cref="UpdateTaskCommand"/>.
/// Ensures that the command contains valid data before it is processed.
/// </summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskCommandValidator"/> class
    /// and sets up validation rules for updateing a task.
    /// </summary>
    public UpdateTaskCommandValidator()
    {
        this.RuleFor(x => x.Dto.Title)
            .NotEmpty().WithMessage("Task title cannot be empty.");
    }
}
