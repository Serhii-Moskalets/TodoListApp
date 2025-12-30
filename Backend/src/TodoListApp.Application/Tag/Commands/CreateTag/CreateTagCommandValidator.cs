using FluentValidation;

namespace TodoListApp.Application.Tag.Commands.CreateTag;

/// <summary>
/// Validator for <see cref="CreateTagCommand"/> using FluentValidation.
/// Ensures that the tag name is not empty and does not exceed the maximum allowed length.
/// </summary>
public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTagCommandValidator"/> class.
    /// </summary>
    public CreateTagCommandValidator()
    {
        this.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name cannot be null or empty.")
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters.");
    }
}
