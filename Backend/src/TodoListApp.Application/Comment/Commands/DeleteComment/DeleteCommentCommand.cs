using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Comment.Commands.DeleteComment;

/// <summary>
/// Represents a command to delete an existing comment.
/// </summary>
public record DeleteCommentCommand(
    Guid CommentId,
    Guid UserId)
    : ICommand;