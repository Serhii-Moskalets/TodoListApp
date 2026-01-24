using FluentValidation;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetUsersWithTaskAccess;

/// <summary>
/// Validator for <see cref="GetUsersWithTaskAccessQuery"/>.
/// </summary>
public class GetUsersWithTaskAccessQueryValidator : AbstractValidator<GetUsersWithTaskAccessQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUsersWithTaskAccessQueryValidator"/> class.
    /// </summary>
    public GetUsersWithTaskAccessQueryValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        this.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
