using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tag.Commands.CreateTag;

/// <summary>
/// Command representing the request to create a new tag for a user.
/// </summary>
public record CreateTagCommand(Guid UserId, Guid TaskId, string? Name) : ICommand<Guid>;
