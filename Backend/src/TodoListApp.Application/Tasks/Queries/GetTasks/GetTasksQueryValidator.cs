using FluentValidation;

namespace TodoListApp.Application.Tasks.Queries.GetTasks;

/// <summary>
/// Validator for <see cref="GetTasksQuery"/>.
/// </summary>
public class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTasksQueryValidator"/> class.
    /// </summary>
    public GetTasksQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        this.RuleFor(x => x.TaskListId)
            .NotEmpty().WithMessage("Task list ID is required.");

        this.RuleFor(x => x.DueAfter)
            .LessThanOrEqualTo(x => x.DueBefore)
            .When(x => x.DueAfter.HasValue && x.DueBefore.HasValue)
            .WithMessage("DueAfter must be before or equal to DueBefore.");

        this.RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        this.RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}