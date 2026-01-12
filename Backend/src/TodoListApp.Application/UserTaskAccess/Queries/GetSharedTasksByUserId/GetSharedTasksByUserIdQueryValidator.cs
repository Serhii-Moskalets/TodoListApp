using FluentValidation;

namespace TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;

/// <summary>
/// Validator for <see cref="GetSharedTasksByUserIdQuery"/>.
/// </summary>
public class GetSharedTasksByUserIdQueryValidator : AbstractValidator<GetSharedTasksByUserIdQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSharedTasksByUserIdQueryValidator"/> class.
    /// </summary>
    public GetSharedTasksByUserIdQueryValidator()
    {
        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
