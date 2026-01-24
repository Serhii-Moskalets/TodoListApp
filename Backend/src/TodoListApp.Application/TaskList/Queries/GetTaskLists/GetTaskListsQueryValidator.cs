using FluentValidation;

namespace TodoListApp.Application.TaskList.Queries.GetTaskLists;

/// <summary>
/// Validator for <see cref="GetTaskListsQuery"/>.
/// </summary>
public class GetTaskListsQueryValidator : AbstractValidator<GetTaskListsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskListsQueryValidator"/> class.
    /// </summary>
    public GetTaskListsQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        this.RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
