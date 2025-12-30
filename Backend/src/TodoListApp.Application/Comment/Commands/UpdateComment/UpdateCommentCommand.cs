using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Comment.Commands.UpdateComment;

/// <summary>
/// Represents a command to update the text of an existing comment.
/// </summary>
public record UpdateCommentCommand(
    Guid TaskId,
    Guid CommentId,
    Guid UserId,
    string? NewText)
    : ICommand;
