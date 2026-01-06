using FluentValidation;

namespace TodoListApp.Application.Tasks.Queries.GetTaskById;

/// <summary>
/// Validator for <see cref="GetTaskByIdQuery"/>.
/// </summary>
public class GetTaskByIdQueryValidator : AbstractValidator<GetTaskByIdQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskByIdQueryValidator"/> class.
    /// </summary>
    public GetTaskByIdQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");
    }
}
