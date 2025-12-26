using FluentValidation;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tag.Commands.DeleteTag;

/// <summary>
/// Validator for <see cref="DeleteTagCommand"/> using FluentValidation.
/// Ensures that the tag exists and belongs to the user attempting to delete it.
/// </summary>
public class DeleteTagCommandValidator : AbstractValidator<DeleteTagCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTagCommandValidator"/> class.
    /// </summary>
    /// <param name="unitOfWork">
    /// The unit of work used to access tag data for validation.
    /// </param>
    public DeleteTagCommandValidator(IUnitOfWork unitOfWork)
    {
        this.RuleFor(x => x.TagId)
            .MustAsync(async (command, id, ct) =>
                await unitOfWork.Tags.IsTagOwnerAsync(id, command.UserId, ct))
            .WithMessage("Tag not found or does not belong to the user.");
    }
}
