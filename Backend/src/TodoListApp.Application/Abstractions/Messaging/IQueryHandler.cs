using TinyResult;

namespace TodoListApp.Application.Abstractions.Messaging;

/// <summary>
/// Handles a query of type <typeparamref name="TQuery"/> and returns a result of type <typeparamref name="TResult"/>.
/// Query handlers are responsible for processing read-only requests without modifying application state.
/// </summary>
/// <typeparam name="TQuery">The type of the query to handle. Must implement <see cref="IQuery{TResult}"/>.</typeparam>
/// <typeparam name="TResult">The type of the result returned by the query.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the specified query asynchronously.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the query result of type <typeparamref name="TResult"/>.</returns>
    Task<Result<TResult>> Handle(TQuery query, CancellationToken cancellationToken);
}
