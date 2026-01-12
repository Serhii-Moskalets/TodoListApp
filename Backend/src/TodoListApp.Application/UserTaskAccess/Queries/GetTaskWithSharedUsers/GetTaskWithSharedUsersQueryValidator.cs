using FluentValidation;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetTaskWithSharedUsers;

/// <summary>
/// Validator for <see cref="GetTaskWithSharedUsersQuery"/>.
/// </summary>
public class GetTaskWithSharedUsersQueryValidator : AbstractValidator<GetTaskWithSharedUsersQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskWithSharedUsersQueryValidator"/> class.
    /// </summary>
    public GetTaskWithSharedUsersQueryValidator()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
