using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tag.Commands.DeleteTag;

/// <summary>
/// Represents a command to delete a tag belonging to a specific user.
/// </summary>
public record DeleteTagCommand(Guid TagId, Guid UserId) : ICommand;
