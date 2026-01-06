using FluentValidation;

namespace TodoListApp.Application.TaskList.Queries.GetAllTaskList;

/// <summary>
/// Validator for <see cref="GetAllTaskListQuery"/>.
/// </summary>
public class GetAllTaskListQueryValidator : AbstractValidator<GetAllTaskListQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllTaskListQueryValidator"/> class.
    /// </summary>
    public GetAllTaskListQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
