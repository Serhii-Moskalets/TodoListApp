using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Api.DTOs.Tag;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tag.Commands.CreateTag;
using TodoListApp.Application.Tag.Commands.DeleteTag;
using TodoListApp.Application.Tag.Queries.GetAllTags;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Controller for managing tags.
/// Provides endpoints to get all tags, create a new tag, and delete an existing tag.
/// </summary>
[ApiController]
[Route("[controller]")]
public class TagsController : BaseController
{
    private readonly ICommandHandler<CreateTagCommand, Guid> _createTagHandler;
    private readonly ICommandHandler<DeleteTagCommand, bool> _deleteTagHandler;
    private readonly IQueryHandler<GetAllTagsQuery, IEnumerable<TagDto>> _getAllTagsHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsController"/> class.
    /// </summary>
    /// <param name="createTagHandler">Handler for creating tags.</param>
    /// <param name="deleteTagHandler">Handler for deleting tags.</param>
    /// <param name="getAllTagsHandler">Handler for retrieving all tags.</param>
    public TagsController(
        ICommandHandler<CreateTagCommand, Guid> createTagHandler,
        ICommandHandler<DeleteTagCommand, bool> deleteTagHandler,
        IQueryHandler<GetAllTagsQuery, IEnumerable<TagDto>> getAllTagsHandler)
    {
        this._createTagHandler = createTagHandler;
        this._deleteTagHandler = deleteTagHandler;
        this._getAllTagsHandler = getAllTagsHandler;
    }

    /// <summary>
    /// Retrieves all tags for the current user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="TagDto"/>.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllTags()
    {
        var query = new GetAllTagsQuery(CurrentUserId);
        var result = await this._getAllTagsHandler.Handle(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Creates a new tag for a specific task.
    /// </summary>
    /// <param name="taskId">The ID of the task to attach the tag to.</param>
    /// <param name="request">The tag creation request containing the tag name.</param>
    /// <returns>An <see cref="IActionResult"/> containing the created tag ID.</returns>
    [HttpPost("{taskId:guid}")]
    public async Task<IActionResult> CreateTag([FromRoute] Guid taskId, [FromBody] TagTitleRequest request)
    {
        var command = new CreateTagCommand(CurrentUserId, taskId, request.Name);
        var result = await this._createTagHandler.Handle(command, this.HttpContext.RequestAborted);
        return this.CreatedAtAction(
            string.Empty,
            new { id = result.Value });
    }

    /// <summary>
    /// Deletes a tag by its ID.
    /// </summary>
    /// <param name="tagId">The ID of the tag to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the operation.</returns>
    [HttpDelete("{tagId:guid}")]
    public async Task<IActionResult> DeleteTag([FromRoute] Guid tagId)
    {
        var command = new DeleteTagCommand(tagId, CurrentUserId);
        var result = await this._deleteTagHandler.Handle(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
