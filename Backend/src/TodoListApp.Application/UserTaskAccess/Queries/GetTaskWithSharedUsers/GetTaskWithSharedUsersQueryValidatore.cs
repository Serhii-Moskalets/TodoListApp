using FluentValidation;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetTaskWithSharedUsers;

/// <summary>
/// Validator for <see cref="GetTaskWithSharedUsersQuery"/>.
/// </summary>
public class GetTaskWithSharedUsersQueryValidatore : AbstractValidator<GetTaskWithSharedUsersQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskWithSharedUsersQueryValidatore"/> class.
    /// </summary>
    public GetTaskWithSharedUsersQueryValidatore()
    {
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
