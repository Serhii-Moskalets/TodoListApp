using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Comment.Commands.CreateComment;

/// <summary>
/// Represents a command to create a new comment for a task.
/// </summary>
public record CreateCommentCommand(Guid TaskId, Guid UserId, string Text)
    : ICommand;
