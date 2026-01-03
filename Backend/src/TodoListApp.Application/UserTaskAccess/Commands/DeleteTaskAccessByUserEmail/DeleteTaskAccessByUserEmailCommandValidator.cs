using System.Text.RegularExpressions;
using FluentValidation;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

/// <summary>
/// Validates the <see cref="DeleteTaskAccessByUserEmailCommand"/> to ensure that
/// a user-task access entry can be deleted only if it exists
/// and the requesting user is the owner of the task.
/// </summary>
public partial class DeleteTaskAccessByUserEmailCommandValidator
    : AbstractValidator<DeleteTaskAccessByUserEmailCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessByUserEmailCommandValidator"/> class.
    /// </summary>
    public DeleteTaskAccessByUserEmailCommandValidator()
    {
        this.RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email cannot be null or empty.")
            .Must(email => EmailRegex().IsMatch(email!))
            .WithMessage("Email address is incorrect.");
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}
