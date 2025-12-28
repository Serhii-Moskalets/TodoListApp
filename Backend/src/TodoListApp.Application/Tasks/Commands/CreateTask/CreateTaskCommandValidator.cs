using FluentValidation;

namespace TodoListApp.Application.Tasks.Commands.CreateTask;

/// <summary>
/// Validator for <see cref="CreateTaskCommand"/>.
/// Ensures that the command contains valid data before it is processed.
/// </summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskCommandValidator"/> class
    /// and sets up validation rules for creating a new task.
    /// </summary>
    public CreateTaskCommandValidator()
    {
        this.RuleFor(x => x.Dto.Title)
            .NotEmpty().WithMessage("Task title cannot be empty.");

        this.RuleFor(x => x.Dto.DueDate)
            .Must((dueDate) => dueDate == null || dueDate.Value >= DateTime.UtcNow)
            .WithMessage("Due date cannot be in the past.");
    }
}
