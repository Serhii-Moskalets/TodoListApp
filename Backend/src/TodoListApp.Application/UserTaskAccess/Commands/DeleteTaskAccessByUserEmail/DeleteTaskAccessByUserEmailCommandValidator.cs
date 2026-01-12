using FluentValidation;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

/// <summary>
/// Validator for <see cref="DeleteTaskAccessByUserEmailCommand"/>.
/// </summary>
public partial class DeleteTaskAccessByUserEmailCommandValidator
    : AbstractValidator<DeleteTaskAccessByUserEmailCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessByUserEmailCommandValidator"/> class.
    /// </summary>
    public DeleteTaskAccessByUserEmailCommandValidator()
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
