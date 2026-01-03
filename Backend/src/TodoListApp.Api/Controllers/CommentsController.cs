using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Api.DTOs.Comment;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Comment.Commands.CreateComment;
using TodoListApp.Application.Comment.Commands.DeleteComment;
using TodoListApp.Application.Comment.Commands.UpdateComment;
using TodoListApp.Application.Comment.Queries.GetComments;
using TodoListApp.Application.Common.Dtos;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing comments related to tasks.
/// </summary>
[ApiController]
[Route("api/tasks/{taskId:guid}/comments")]
public class CommentsController : BaseController
{
    private readonly ICommandHandler<CreateCommentCommand, Guid> _createCommentHandler;
    private readonly ICommandHandler<DeleteCommentCommand, bool> _deleteCommentHandler;
    private readonly ICommandHandler<UpdateCommentCommand, bool> _updateCommentHandler;
    private readonly IQueryHandler<GetCommentsQuery, IEnumerable<CommentDto>> _getCommentsHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentsController"/> class.
    /// </summary>
    /// <param name="createCommandHandler">Handler responsible for creating comments.</param>
    /// <param name="deleteCommentHandler">Handler responsible for deleting comments.</param>
    /// <param name="updateCommentHandler">Handler responsible for updating comments.</param>
    /// <param name="getCommentsHandler">Handler responsible for retrieving comments for a task.</param>
    public CommentsController(
        ICommandHandler<CreateCommentCommand, Guid> createCommandHandler,
        ICommandHandler<DeleteCommentCommand, bool> deleteCommentHandler,
        ICommandHandler<UpdateCommentCommand, bool> updateCommentHandler,
        IQueryHandler<GetCommentsQuery, IEnumerable<CommentDto>> getCommentsHandler)
    {
        this._createCommentHandler = createCommandHandler;
        this._deleteCommentHandler = deleteCommentHandler;
        this._updateCommentHandler = updateCommentHandler;
        this._getCommentsHandler = getCommentsHandler;
    }

    /// <summary>
    /// Retrieves all comments associated with a specific task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <returns>A collection of comments related to the specified task.</returns>
    [HttpGet]
    public async Task<IActionResult> GetComments([FromRoute] Guid taskId)
    {
        var query = new GetCommentsQuery(taskId, CurrentUserId);
        var result = await this._getCommentsHandler.Handle(query, this.HttpContext.RequestAborted);
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
        var result = await this._createCommentHandler.Handle(command, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Deletes an existing comment.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="commentId">The unique identifier of the comment to delete.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the comment was successfully deleted;
    /// otherwise, returns <see cref="BadRequestObjectResult"/> with error details.
    /// </returns>
    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(
        [FromRoute] Guid taskId,
        [FromRoute] Guid commentId)
    {
        var command = new DeleteCommentCommand(taskId, commentId, CurrentUserId);
        var result = await this._deleteCommentHandler.Handle(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Updates the text of an existing comment.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <param name="request">The request containing the updated comment text.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the comment was successfully updated;
    /// otherwise, returns <see cref="BadRequestObjectResult"/> with error details.
    /// </returns>
    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(
        [FromRoute] Guid taskId,
        [FromRoute] Guid commentId,
        [FromBody] CommentTextRequest request)
    {
        var command = new UpdateCommentCommand(taskId, commentId, CurrentUserId, request.Text);
        var result = await this._updateCommentHandler.Handle(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
