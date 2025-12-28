using FluentValidation;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;

namespace TodoListApp.Application.TaskList.Commands.CreateTaskList;

/// <summary>
/// Validates the <see cref="CreateTaskListCommand"/> to ensure all required properties
/// meet the defined business rules.
/// </summary>
public class CreateTaskListCommandValidator : AbstractValidator<CreateTaskListCommand>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskListCommandValidator"/> class.
    /// </summary>
    /// <param name="userRepository">
    /// The repository used to check the existence of the owner user in the system.
    /// </param>
    public CreateTaskListCommandValidator(IUserRepository userRepository)
    {
        this._userRepository = userRepository;

        this.RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be null or empty.");

        this.RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("OwnerId cannot be empty.")
            .MustAsync(async (userId, cancellationToken) =>
                await this._userRepository.ExistsAsync(userId, cancellationToken))
            .WithMessage("Owner user does not exist.");
    }
}
