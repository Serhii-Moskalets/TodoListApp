using FluentValidation;

namespace TodoListApp.Application.Tag.Queries.GetAllTags;

/// <summary>
/// Validator for <see cref="GetAllTagsQuery"/>.
/// </summary>
public class GetAllTagsQueryValidator : AbstractValidator<GetAllTagsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllTagsQueryValidator"/> class.
    /// </summary>
    public GetAllTagsQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
