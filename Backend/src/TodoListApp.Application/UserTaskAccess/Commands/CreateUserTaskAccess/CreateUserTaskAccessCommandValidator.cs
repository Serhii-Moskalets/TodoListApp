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
        this.RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");
        this.RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");
        this.RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email cannot be null or empty.")
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
                .WithMessage("Email address is incorrect.");
    }
}
