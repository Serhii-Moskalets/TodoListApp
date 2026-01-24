using FluentValidation;

namespace TodoListApp.Application.Tasks.Commands.UpdateTask;

/// <summary>
/// Validator for <see cref="UpdateTaskCommand"/>.
/// Ensures that the command contains valid data before it is processed.
/// </summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskCommandValidator"/> class
    /// and sets up validation rules for updating a task.
    /// </summary>
    public UpdateTaskCommandValidator()
    {
        this.RuleFor(x => x.Dto.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.Dto.Title)
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        this.RuleFor(x => x.Dto.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        this.RuleFor(x => x.Dto.DueDate)
            .Must((dueDate) => dueDate == null || dueDate.Value >= DateTime.UtcNow)
            .WithMessage("Due date cannot be in the past.");
    }
}
