using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Api.DTOs.Task;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Commands.AddTagToTask;
using TodoListApp.Application.Tasks.Commands.ChangeTaskStatus;
using TodoListApp.Application.Tasks.Commands.CreateTask;
using TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;
using TodoListApp.Application.Tasks.Commands.DeleteTask;
using TodoListApp.Application.Tasks.Commands.RemoveTagFromTask;
using TodoListApp.Application.Tasks.Commands.UpdateTask;
using TodoListApp.Application.Tasks.Dtos;
using TodoListApp.Application.Tasks.Queries.GetTaskById;
using TodoListApp.Application.Tasks.Queries.GetTaskByTitle;
using TodoListApp.Application.Tasks.Queries.GetTasks;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing tasks.
/// </summary>
/// <remarks>
/// This controller acts as an API layer and delegates
/// all business logic to application command and query handlers.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class TasksController : BaseController
{
    private readonly ICommandHandler<CreateTaskCommand, Guid> _createTaskHandler;
    private readonly ICommandHandler<UpdateTaskCommand, bool> _updateTaskHandler;
    private readonly ICommandHandler<DeleteTaskCommand, bool> _deleteTaskHandler;
    private readonly ICommandHandler<AddTagToTaskCommand, bool> _addTagToTaskHandler;
    private readonly ICommandHandler<RemoveTagFromTaskCommand, bool> _removeTagFromTaskHandler;
    private readonly ICommandHandler<ChangeTaskStatusCommand, bool> _changeTaskStatusHandler;
    private readonly IQueryHandler<GetTaskByIdQuery, TaskDto> _getTaskByIdHandler;
    private readonly IQueryHandler<GetTaskByTitleQuery, IEnumerable<TaskDto>> _getTaskByTitleHandler;
    private readonly IQueryHandler<GetTasksQuery, IEnumerable<TaskDto>> _getTasksHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TasksController"/> class.
    /// </summary>
    /// <param name="createTaskHandler">Handler to create new tasks.</param>
    /// <param name="getTaskByIdHandler">Handler to get a task by its ID.</param>
    /// <param name="getTaskByTitleHandler">Handler to get tasks by title.</param>
    /// <param name="getTasksHandler">Handler to get all tasks with optional filters.</param>
    /// <param name="updateTaskHandler">Handler to update an existing task.</param>
    /// <param name="deleteTaskHandler">Handler to delete a task.</param>
    /// <param name="removeTagFromTaskHandler">Handler to remove a tag from a task.</param>
    /// <param name="changeTaskStatusHandler">Handler to change the status of a task.</param>
    /// <param name="addTagToTaskHandler">Handler to add a tag to a task.</param>
    public TasksController(
        ICommandHandler<CreateTaskCommand, Guid> createTaskHandler,
        IQueryHandler<GetTaskByIdQuery, TaskDto> getTaskByIdHandler,
        IQueryHandler<GetTaskByTitleQuery, IEnumerable<TaskDto>> getTaskByTitleHandler,
        IQueryHandler<GetTasksQuery, IEnumerable<TaskDto>> getTasksHandler,
        ICommandHandler<UpdateTaskCommand, bool> updateTaskHandler,
        ICommandHandler<DeleteTaskCommand, bool> deleteTaskHandler,
        ICommandHandler<RemoveTagFromTaskCommand, bool> removeTagFromTaskHandler,
        ICommandHandler<ChangeTaskStatusCommand, bool> changeTaskStatusHandler,
        ICommandHandler<AddTagToTaskCommand, bool> addTagToTaskHandler)
    {
        this._createTaskHandler = createTaskHandler;
        this._getTaskByIdHandler = getTaskByIdHandler;
        this._getTaskByTitleHandler = getTaskByTitleHandler;
        this._getTasksHandler = getTasksHandler;
        this._updateTaskHandler = updateTaskHandler;
        this._deleteTaskHandler = deleteTaskHandler;
        this._removeTagFromTaskHandler = removeTagFromTaskHandler;
        this._changeTaskStatusHandler = changeTaskStatusHandler;
        this._addTagToTaskHandler = addTagToTaskHandler;
    }

    /// <summary>
    /// Retrieves a task by its identifier.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <returns>The requested task if found.</returns>
    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetTaskById([FromRoute] Guid taskId)
    {
        var query = new GetTaskByIdQuery(CurrentUserId, taskId);
        var result = await this._getTaskByIdHandler.Handle(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Retrieves all tasks for the current user with optional filters.
    /// </summary>
    /// <param name="request">Filtering and sorting parameters.</param>
    /// <returns>A list of tasks.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllTasks([FromQuery] GetTasksRequest request)
    {
        var query = new GetTasksQuery(
            CurrentUserId,
            request.TaskListId,
            request.TaskStatuses,
            request.DueBefore,
            request.DueAfter,
            request.TaskSortBy,
            request.Ascending);

        var result = await this._getTasksHandler.Handle(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Retrieves tasks that match the specified title.
    /// </summary>
    /// <param name="request">The request containing the task title.</param>
    /// <returns>A list of matching tasks.</returns>
    [HttpGet("by-title")]
    public async Task<IActionResult> GetTasksByTitle([FromQuery] GetTaskByTitleRequest request)
    {
        var query = new GetTaskByTitleQuery(CurrentUserId, request.Title);
        var result = await this._getTaskByTitleHandler.Handle(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list.</param>
    /// <param name="request">The task creation request.</param>
    /// <returns>
    /// An <see cref="OkObjectResult"/> containing the created task identifier
    /// if the operation succeeds; otherwise, a <see cref="BadRequestObjectResult"/>
    /// containing error details.
    /// </returns>
    [HttpPost("task-lists/{taskListId:guid}/tasks")]
    public async Task<IActionResult> CreateTask([FromRoute] Guid taskListId, [FromBody] CreateTaskDtoRequest request)
    {
        var command = new CreateTaskCommand(
            new CreateTaskDto
            {
                Title = request.Title,
                DueDate = request.DueDate,
                TaskListId = taskListId,
            }, CurrentUserId);

        var result = await this._createTaskHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="request">The task update request.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the task was successfully updated;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> UpdateTask([FromRoute] Guid taskId, [FromBody] UpdateTaskDtoRequest request)
    {
        var command = new UpdateTaskCommand(
            new UpdateTaskDto
            {
                TaskId = taskId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
            }, CurrentUserId);

        var result = await this._updateTaskHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Deletes a task by its identifier.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> DeleteTask([FromRoute] Guid taskId)
    {
        var command = new DeleteTaskCommand(taskId, CurrentUserId);
        var result = await this._deleteTaskHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Adds a tag to the specified task.
    /// </summary>
    /// <param name="taskId">The task identifier..</param>
    /// <param name="tagId">The tag identifier.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpPut("{taskId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> AddTagToTask([FromRoute] Guid taskId, [FromRoute] Guid tagId)
    {
        var command = new AddTagToTaskCommand(taskId, CurrentUserId, tagId);
        var result = await this._addTagToTaskHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Removes the single tag from a task.
    /// Each task can only have one tag.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpDelete("{taskId:guid}/tag")]
    public async Task<IActionResult> RemoveTagFromTask([FromRoute] Guid taskId)
    {
        var command = new RemoveTagFromTaskCommand(taskId, CurrentUserId);
        var result = await this._removeTagFromTaskHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Changes the status of a task.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="status">The status of task.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpPut("{taskId:guid}/status/{status:int}")]
    public async Task<IActionResult> ChangeTaskStatus([FromRoute] Guid taskId, [FromRoute] StatusTask status)
    {
        var command = new ChangeTaskStatusCommand(taskId, CurrentUserId, status);
        var result = await this._changeTaskStatusHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
