using TinyResult;

namespace TodoListApp.Application.Abstractions.Messaging;

/// <summary>
/// Handles a specific command of type <typeparamref name="TCommand"/>.
/// </summary>
/// <typeparam name="TCommand">The type of the command to handle. Must implement <see cref="ICommand"/>.</typeparam>
internal interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
    /// containing a <see cref="Result{Boolean}"/> that indicates success or failure of the command.
    /// </returns>
    Task<Result<bool>> Handle(TCommand command, CancellationToken cancellationToken);
}
