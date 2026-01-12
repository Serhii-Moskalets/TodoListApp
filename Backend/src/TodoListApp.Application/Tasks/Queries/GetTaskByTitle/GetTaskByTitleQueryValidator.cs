using FluentValidation;

namespace TodoListApp.Application.Tasks.Queries.GetTaskByTitle;

/// <summary>
/// Validator for <see cref="GetTaskByTitleQuery"/>.
/// </summary>
public class GetTaskByTitleQueryValidator : AbstractValidator<GetTaskByTitleQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskByTitleQueryValidator"/> class.
    /// </summary>
    public GetTaskByTitleQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required.");

        this.RuleFor(x => x.Text)
            .MaximumLength(100).WithMessage("Search text cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Text));
    }
}
