using FluentValidation;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;

/// <summary>
/// Validator for <see cref="GetSharedTasksByUserIdQuery"/>.
/// </summary>
public class GetSharedTasksByUserIdQueryValidator : AbstractValidator<GetSharedTasksByUserIdQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSharedTasksByUserIdQueryValidator"/> class.
    /// </summary>
    public GetSharedTasksByUserIdQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        this.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
