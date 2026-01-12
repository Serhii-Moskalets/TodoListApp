using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Abstractions.Messaging;

/// <summary>
/// Abstract base class for all handlers (commands and queries) in the CQRS pattern.
/// Provides shared logic and services that all handlers may need,
/// such as access to <see cref="IUnitOfWork"/> for repository operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HandlerBase"/> class with the specified unit of work.
/// </remarks>
/// <param name="unitOfWork">An <see cref="IUnitOfWork"/> instance for working with repositories.</param>
public abstract class HandlerBase(IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Gets the unit of work for accessing repositories.
    /// </summary>
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;

    /// <summary>
    /// Performs validation of the given command or query using FluentValidation.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command or query to validate.</typeparam>
    /// <param name="validator">
    /// The validator instance for the specific command or query type.
    /// If <c>null</c> is provided, validation is skipped.
    /// </param>
    /// <param name="command">The command or query instance to validate.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing <c>true</c> if validation succeeds,
    /// or <c>Failure</c> with the first error message if validation fails.
    /// </returns>
    protected static async Task<Result<bool>> ValidateAsync<TCommand>(IValidator<TCommand>? validator, TCommand command)
    {
        if (validator is null)
        {
            return await Result<bool>.SuccessAsync(true);
        }

        var result = await validator.ValidateAsync(command);
        if (!result.IsValid)
        {
            return await Result<bool>.FailureAsync(
                TinyResult.Enums.ErrorCode.ValidationError,
                result.Errors[0].ErrorMessage);
        }

        return await Result<bool>.SuccessAsync(true);
    }
}
