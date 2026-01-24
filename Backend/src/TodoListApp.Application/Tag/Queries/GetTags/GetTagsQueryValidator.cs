using FluentValidation;

namespace TodoListApp.Application.Tag.Queries.GetTags;

/// <summary>
/// Validator for <see cref="GetTagsQuery"/>.
/// </summary>
public class GetTagsQueryValidator : AbstractValidator<GetTagsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagsQueryValidator"/> class.
    /// </summary>
    public GetTagsQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        this.RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
