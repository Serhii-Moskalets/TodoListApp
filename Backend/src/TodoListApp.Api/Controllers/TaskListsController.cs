using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Api.Requests.TaskList;
using TodoListApp.Application.TaskList.Commands.CreateTaskList;
using TodoListApp.Application.TaskList.Commands.DeleteTaskList;
using TodoListApp.Application.TaskList.Commands.UpdateTaskList;
using TodoListApp.Application.TaskList.Dtos;
using TodoListApp.Application.TaskList.Queries.GetTaskLists;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Provides API endpoints to manage task lists for the current user.
/// Supports creating, updating, deleting, and retrieving task lists.
/// </summary>
public class TaskListsController : BaseController
{
    /// <summary>
    /// Retrieves all task lists for the current user.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="TaskListDto"/> or an error.</returns>
    [HttpGet]
    public async Task<IActionResult> GetTaskLists([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetTaskListsQuery(CurrentUserId, page, pageSize);
        var result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
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
    [HttpPost]
    public async Task<IActionResult> CreateTaskList([FromBody] TaskListTitleRequest request)
    {
        var command = new CreateTaskListCommand(CurrentUserId, request.Title);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
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
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
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
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
