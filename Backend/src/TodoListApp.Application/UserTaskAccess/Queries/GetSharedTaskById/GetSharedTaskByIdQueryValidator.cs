using FluentValidation;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTaskById;

/// <summary>
/// Validator for <see cref="GetSharedTaskByIdQuery"/>.
/// </summary>
public class GetSharedTaskByIdQueryValidator : AbstractValidator<GetSharedTaskByIdQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSharedTaskByIdQueryValidator"/> class.
    /// </summary>
    public GetSharedTaskByIdQueryValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
