namespace TodoListApp.Api.Requests.Comment;

/// <summary>
/// Represents the request data containing the text of a comment.
/// Used for creating or updating a comment.
/// </summary>
/// <param name="Text">
/// The content of the comment.
/// Can be <see langword="null"/> if no text is provided.
/// </param>
public record CommentTextRequest(string? Text);
