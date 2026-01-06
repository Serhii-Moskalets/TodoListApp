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
            .NotEmpty().WithMessage("User ID is required");

        this.RuleFor(x => x.TaskListId)
            .NotEmpty().WithMessage("Task list ID is required");

        this.RuleFor(x => x)
            .Must(x => !x.DueAfter.HasValue || !x.DueBefore.HasValue || x.DueAfter <= x.DueBefore)
            .WithMessage("DueAfter must be before or equal to DueBefore");
    }
}