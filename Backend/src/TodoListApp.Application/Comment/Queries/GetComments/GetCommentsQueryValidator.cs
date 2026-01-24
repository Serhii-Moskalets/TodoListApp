using FluentValidation;

namespace TodoListApp.Application.Comment.Queries.GetComments;

/// <summary>
/// Validator for <see cref="GetCommentsQuery"/>.
/// </summary>
public class GetCommentsQueryValidator : AbstractValidator<GetCommentsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetCommentsQueryValidator"/> class.
    /// </summary>
    public GetCommentsQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required.");

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        this.RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
