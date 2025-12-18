using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Abstractions.Messaging;

/// <summary>
/// Abstract base class for all handlers (commands and queries) in the CQRS pattern.
/// Provides shared logic and services that all handlers may need,
/// such as access to <see cref="IUnitOfWork"/> for repository operations.
/// </summary>
public abstract class HandlerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerBase"/> class with the specified unit of work.
    /// </summary>
    /// <param name="unitOfWork">An <see cref="IUnitOfWork"/> instance for working with repositories.</param>
    protected HandlerBase(IUnitOfWork unitOfWork)
    {
        this.UnitOfWork = unitOfWork;
    }

    /// <summary>
    /// Gets the unit of work for accessing repositories.
    /// </summary>
    protected IUnitOfWork UnitOfWork { get; }
}
