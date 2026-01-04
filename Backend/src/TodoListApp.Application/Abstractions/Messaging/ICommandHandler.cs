using TinyResult;

namespace TodoListApp.Application.Abstractions.Messaging;

/// <summary>
/// Handles a specific command of type <typeparamref name="TCommand"/>.
/// </summary>
/// <typeparam name="TCommand">The type of the command to handle. Must implement <see cref="ICommand"/>.</typeparam>
/// <typeparam name="TResult">The type of result returned by the handler.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
    /// containing a <see cref="Result{TResult}"/> that indicates success or failure of the command.
    /// </returns>
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
