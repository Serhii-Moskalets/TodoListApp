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
public class TaskListController : BaseController
{
    private readonly IQueryHandler<GetAllTaskListQuery, IEnumerable<TaskListDto>> _getAllHandler;
    private readonly ICommandHandler<CreateTaskListCommand> _createHandler;
    private readonly ICommandHandler<DeleteTaskListCommand> _deleteHandler;
    private readonly ICommandHandler<UpdateTaskListCommand> _updateHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskListController"/> class.
    /// </summary>
    /// <param name="createHandler">Handler for creating task lists.</param>
    /// <param name="getAllHandler">Handler for retrieving all task lists.</param>
    /// <param name="deleteHandler">Handler for deleting task lists.</param>
    /// <param name="updateHandler">Handler for updating task lists.</param>
    public TaskListController(
        ICommandHandler<CreateTaskListCommand> createHandler,
        IQueryHandler<GetAllTaskListQuery, IEnumerable<TaskListDto>> getAllHandler,
        ICommandHandler<DeleteTaskListCommand> deleteHandler,
        ICommandHandler<UpdateTaskListCommand> updateHandler)
    {
        this._createHandler = createHandler;
        this._getAllHandler = getAllHandler;
        this._deleteHandler = deleteHandler;
        this._updateHandler = updateHandler;
    }

    /// <summary>
    /// Retrieves all task lists for the current user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="TaskListDto"/> or an error.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllTaskListQuery(CurrentUserId);
        var result = await this._getAllHandler.Handle(query, this.HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return this.BadRequest(result.Error);
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Creates a new task list for the current user.
    /// </summary>
    /// <param name="request">The task list creation request containing the title.</param>
    /// <returns>An <see cref="IActionResult"/> with the created task list DTO or an error.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskListRequest request)
    {
        var command = new CreateTaskListCommand(CurrentUserId, request.Title);
        var result = await this._createHandler.Handle(command, this.HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return this.BadRequest(result.Error);
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Deletes a task list for the current user by its ID.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating success or failure.</returns>
    [HttpDelete("{taskListId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid taskListId)
    {
        var command = new DeleteTaskListCommand(taskListId, CurrentUserId);
        var result = await this._deleteHandler.Handle(command, this.HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return this.BadRequest(result.Error);
        }

        return this.Ok(result.Value);
    }

    /// <summary>
    /// Updates the title of an existing task list for the current user.
    /// </summary>
    /// <param name="request">The update request containing the task list ID and the new title.</param>
    /// <returns>An <see cref="IActionResult"/> with the updated task list DTO or an error.</returns>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateTaskListRequest request)
    {
        var command = new UpdateTaskListCommand(request.TaskListId, CurrentUserId, request.NewTitle);
        var result = await this._updateHandler.Handle(command, this.HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return this.BadRequest(result.Error);
        }

        return this.Ok(result.Value);
    }
}
