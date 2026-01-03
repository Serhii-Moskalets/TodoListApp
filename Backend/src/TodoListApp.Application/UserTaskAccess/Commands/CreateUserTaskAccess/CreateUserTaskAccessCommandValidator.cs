using System.Text.RegularExpressions;
using FluentValidation;

namespace TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;

/// <summary>
/// Validator for <see cref="CreateUserTaskAccessCommand"/>.
/// </summary>
public class CreateUserTaskAccessCommandValidator : AbstractValidator<CreateUserTaskAccessCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserTaskAccessCommandValidator"/> class
    /// and configures validation rules for creating user-task access.
    /// </summary>
    public CreateUserTaskAccessCommandValidator()
    {
        this.RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email cannot be null or empty.")
            .Must(email => Regex.IsMatch(email!, @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$"))
                .WithMessage("Email address is incorrect.");
    }
}
