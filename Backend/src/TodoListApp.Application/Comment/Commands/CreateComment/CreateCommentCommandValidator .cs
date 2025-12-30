using FluentValidation;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Comment.Commands.CreateComment;

/// <summary>
/// Validator for <see cref="CreateCommentCommand"/> using FluentValidation.
/// Ensures that the comment text is not empty and does not exceed the maximum allowed length.
/// </summary>
public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentCommandValidator"/> class.
    /// </summary>
    /// <param name="unitOfWork">
    /// The unit of work used to access the tasks repository for validation.
    /// </param>
    public CreateCommentCommandValidator(IUnitOfWork unitOfWork)
    {
        this.RuleFor(x => x.TaskId)
            .MustAsync(async (taskId, ct) =>
                await unitOfWork.Tasks.ExistsAsync(taskId, ct))
            .WithMessage("Task not found.");

        this.RuleFor(c => c.Text)
            .NotEmpty().WithMessage("Comment text cannot be null or empty.")
            .MaximumLength(1000).WithMessage("Comment text cannot exceed 1000 characters.");
    }
}
