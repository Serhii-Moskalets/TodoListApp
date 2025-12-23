namespace TodoListApp.Application.Abstractions.Messaging;

/// <summary>
/// Represents a query that returns a result of type <typeparamref name="TResult"/>.
/// Queries are read-only operations that retrieve data without modifying the state.
/// </summary>
/// <typeparam name="TResult">The type of the result returned by the query.</typeparam>
public interface IQuery<TResult> { }
