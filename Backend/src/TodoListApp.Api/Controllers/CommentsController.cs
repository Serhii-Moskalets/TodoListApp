using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Api.Requests.Comment;
using TodoListApp.Application.Comment.Commands.CreateComment;
using TodoListApp.Application.Comment.Commands.DeleteComment;
using TodoListApp.Application.Comment.Commands.UpdateComment;
using TodoListApp.Application.Comment.Queries.GetComments;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing comments related to tasks.
/// </summary>
[Route("api/tasks/{taskId:guid}/comments")]
public class CommentsController : BaseController
{
    /// <summary>
    /// Retrieves all comments associated with a specific task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A collection of comments related to the specified task.</returns>
    [HttpGet]
    public async Task<IActionResult> GetComments([FromRoute] Guid taskId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetCommentsQuery(taskId, CurrentUserId, page, pageSize);
        var result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Creates a new comment for a specific task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="request">The request containing the comment text.</param>
    /// <returns>
    /// A <see cref="CreatedAtActionResult"/> containing the created comment identifier
    /// if the operation succeeds; otherwise, a <see cref="BadRequestObjectResult"/>
    /// containing error details.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateComment(
        [FromRoute] Guid taskId,
        [FromBody] CommentTextRequest request)
    {
        var command = new CreateCommentCommand(taskId, CurrentUserId, request.Text);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Deletes an existing comment.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment to delete.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the comment was successfully deleted;
    /// otherwise, returns <see cref="BadRequestObjectResult"/> with error details.
    /// </returns>
    [HttpDelete("~/api/comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment([FromRoute] Guid commentId)
    {
        var command = new DeleteCommentCommand(commentId, CurrentUserId);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Updates the text of an existing comment.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <param name="request">The request containing the updated comment text.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the comment was successfully updated;
    /// otherwise, returns <see cref="BadRequestObjectResult"/> with error details.
    /// </returns>
    [HttpPut("~/api/comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(
        [FromRoute] Guid commentId,
        [FromBody] CommentTextRequest request)
    {
        var command = new UpdateCommentCommand(commentId, CurrentUserId, request.Text);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
