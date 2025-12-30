using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Api.DTOs.TaskList;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.TaskList.Commands.CreateTaskList;
using TodoListApp.Application.TaskList.Commands.DeleteTaskList;
using TodoListApp.Application.TaskList.Commands.UpdateTaskList;
using TodoListApp.Application.TaskList.Dtos;
using TodoListApp.Application.TaskList.Queries.GetAllTaskList;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Provides API endpoints to manage task lists for the current user.
/// Supports creating, updating, deleting, and retrieving task lists.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TaskListsController : BaseController
{
    private readonly IQueryHandler<GetAllTaskListQuery, IEnumerable<TaskListDto>> _getAllTaskListsHandler;
    private readonly ICommandHandler<CreateTaskListCommand, Guid> _createTaskListHandler;
    private readonly ICommandHandler<DeleteTaskListCommand, bool> _deleteTaskListHandler;
    private readonly ICommandHandler<UpdateTaskListCommand, bool> _updateTaskListHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskListsController"/> class.
    /// </summary>
    /// <param name="createTaskListHandler">Handler for creating task lists.</param>
    /// <param name="getAllTaskListsHandler">Handler for retrieving all task lists.</param>
    /// <param name="deleteTaskListHandler">Handler for deleting task lists.</param>
    /// <param name="updateTaskListHandler">Handler for updating task lists.</param>
    public TaskListsController(
        ICommandHandler<CreateTaskListCommand, Guid> createTaskListHandler,
        IQueryHandler<GetAllTaskListQuery, IEnumerable<TaskListDto>> getAllTaskListsHandler,
        ICommandHandler<DeleteTaskListCommand, bool> deleteTaskListHandler,
        ICommandHandler<UpdateTaskListCommand, bool> updateTaskListHandler)
    {
        this._createTaskListHandler = createTaskListHandler;
        this._getAllTaskListsHandler = getAllTaskListsHandler;
        this._deleteTaskListHandler = deleteTaskListHandler;
        this._updateTaskListHandler = updateTaskListHandler;
    }

    /// <summary>
    /// Retrieves all task lists for the current user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="TaskListDto"/> or an error.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllTaskLists()
    {
        var query = new GetAllTaskListQuery(CurrentUserId);
        var result = await this._getAllTaskListsHandler.Handle(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Creates a new task list for the current user.
    /// </summary>
    /// <param name="request">The task list creation request containing the title.</param>
    /// <returns>
    /// A <see cref="CreatedAtActionResult"/> containing the created task list identifier
    /// if the operation succeeds; otherwise, a <see cref="BadRequestObjectResult"/>
    /// containing error details.
    /// </returns>
    /// <response code="201">The task list was successfully created.</response>
    /// <response code="400">The request is invalid.</response>
    [HttpPost]
    public async Task<IActionResult> CreateTaskList([FromBody] TaskListTitleRequest request)
    {
        var command = new CreateTaskListCommand(CurrentUserId, request.Title);
        var result = await this._createTaskListHandler.Handle(command, this.HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return this.HandleResult(result);
        }

        return this.CreatedAtAction(
            nameof(this.GetAllTaskLists),
            new { id = result.Value });
    }

    /// <summary>
    /// Deletes a task list for the current user by its ID.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list to delete.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpDelete("{taskListId:guid}")]
    public async Task<IActionResult> DeleteTaskList([FromRoute] Guid taskListId)
    {
        var command = new DeleteTaskListCommand(taskListId, CurrentUserId);
        var result = await this._deleteTaskListHandler.Handle(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Updates the title of an existing task list.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list to update.</param>
    /// <param name="request">The request containing the new task list title.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the task list was successfully updated;
    /// otherwise, returns a <see cref="BadRequestObjectResult"/> with error details.
    /// </returns>
    [HttpPut("{taskListId:guid}")]
    public async Task<IActionResult> UpdateTaskList([FromRoute] Guid taskListId, [FromBody] TaskListTitleRequest request)
    {
        var command = new UpdateTaskListCommand(taskListId, CurrentUserId, request.Title);
        var result = await this._updateTaskListHandler.Handle(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
