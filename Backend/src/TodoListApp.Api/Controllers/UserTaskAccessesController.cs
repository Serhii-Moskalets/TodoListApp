using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Api.DTOs.UserTaskAccess;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;
using TodoListApp.Application.UserTaskAccess.Dtos;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTaskById;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;
using TodoListApp.Application.UserTaskAccess.Queries.GetTaskWithSharedUsers;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Controller responsible for managing user-task access operations.
/// Provides endpoints to retrieve shared tasks, get users with access to a task,
/// and manage access assignments and deletions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserTaskAccessesController : BaseController
{
    private readonly IQueryHandler<GetSharedTaskByIdQuery, TaskDto> _getSharedTaskByIdHandler;
    private readonly IQueryHandler<GetSharedTasksByUserIdQuery, IEnumerable<TaskDto>> _getSharedTasksByUserIdHandler;
    private readonly IQueryHandler<GetTaskWithSharedUsersQuery, TaskAccessListDto> _getTaskWithSharedUsersHandler;
    private readonly ICommandHandler<CreateUserTaskAccessCommand, bool> _createUserTaskAccessHandler;
    private readonly ICommandHandler<DeleteTaskAccessesByTaskCommand, bool> _deleteTaskAccessesByTaskHandler;
    private readonly ICommandHandler<DeleteTaskAccessesByUserCommand, bool> _deleteTaskAccessesByUserHandler;
    private readonly ICommandHandler<DeleteTaskAccessByUserEmailCommand, bool> _deleteTaskAccessByUserEmailHandler;
    private readonly ICommandHandler<DeleteTaskAccessByIdCommand, bool> _deleteTaskAccessByIdHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserTaskAccessesController"/> class.
    /// </summary>
    /// <param name="getSharedTaskByIdHandler">Handler for retrieving a shared task by its ID.</param>
    /// <param name="getSharedTasksByUserIdHandler">Handler for retrieving all tasks shared with a user.</param>
    /// <param name="getTaskWithSharedUsersHandler">Handler for retrieving task details along with users who have access.</param>
    /// <param name="createUserTaskAccessHandler">Handler for creating a new user-task access.</param>
    /// <param name="deletTaskAccessesByTaskHadler">Handler for deleting all task accesses by task ID.</param>
    /// <param name="deleteTaskAccessesByUserHandler">Handler for deleting all task accesses by user ID.</param>
    /// <param name="deleteTaskAccessByUserEmailHandler">Handler for deleting a task access by user email.</param>
    /// <param name="deleteTaskAccessByIdHandler">Handler for deleting a task access by its ID.</param>
    public UserTaskAccessesController(
        IQueryHandler<GetSharedTaskByIdQuery, TaskDto> getSharedTaskByIdHandler,
        IQueryHandler<GetSharedTasksByUserIdQuery, IEnumerable<TaskDto>> getSharedTasksByUserIdHandler,
        IQueryHandler<GetTaskWithSharedUsersQuery, TaskAccessListDto> getTaskWithSharedUsersHandler,
        ICommandHandler<CreateUserTaskAccessCommand, bool> createUserTaskAccessHandler,
        ICommandHandler<DeleteTaskAccessesByTaskCommand, bool> deletTaskAccessesByTaskHadler,
        ICommandHandler<DeleteTaskAccessesByUserCommand, bool> deleteTaskAccessesByUserHandler,
        ICommandHandler<DeleteTaskAccessByUserEmailCommand, bool> deleteTaskAccessByUserEmailHandler,
        ICommandHandler<DeleteTaskAccessByIdCommand, bool> deleteTaskAccessByIdHandler)
    {
        this._getSharedTaskByIdHandler = getSharedTaskByIdHandler;
        this._getSharedTasksByUserIdHandler = getSharedTasksByUserIdHandler;
        this._getTaskWithSharedUsersHandler = getTaskWithSharedUsersHandler;
        this._createUserTaskAccessHandler = createUserTaskAccessHandler;
        this._deleteTaskAccessesByTaskHandler = deletTaskAccessesByTaskHadler;
        this._deleteTaskAccessesByUserHandler = deleteTaskAccessesByUserHandler;
        this._deleteTaskAccessByUserEmailHandler = deleteTaskAccessByUserEmailHandler;
        this._deleteTaskAccessByIdHandler = deleteTaskAccessByIdHandler;
    }

    /// <summary>
    /// Retrieves a shared task by its ID for the current user.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <returns>An <see cref="IActionResult"/> containing the task details if found; otherwise, an error response.</returns>
    [HttpGet("tasks/{taskId:guid}/shared")]
    public async Task<IActionResult> GetSharedTaskByIdAsync([FromRoute] Guid taskId)
    {
        var query = new GetSharedTaskByIdQuery(taskId, CurrentUserId);
        var result = await this._getSharedTaskByIdHandler.Handle(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Retrieves all tasks that are shared with the current user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing a list of tasks shared with the user.</returns>
    [HttpGet]
    public async Task<IActionResult> GetSharedTasksByUserIdAsync()
    {
        var query = new GetSharedTasksByUserIdQuery(CurrentUserId);
        var result = await this._getSharedTasksByUserIdHandler.Handle(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Retrieves a task along with all users who have access to it.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <returns>An <see cref="IActionResult"/> containing the task access list details.</returns>
    [HttpGet("tasks/{taskId:guid}/users")]
    public async Task<IActionResult> GetTaskWithSharedUsersAsync([FromRoute] Guid taskId)
    {
        var query = new GetTaskWithSharedUsersQuery(taskId, CurrentUserId);
        var result = await this._getTaskWithSharedUsersHandler.Handle(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Grants access to a user for a specific task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="request">Request containing the email of the user to share the task with.</param>
    /// <returns>An <see cref="IActionResult"/> indicating success or failure of the operation.</returns>
    [HttpPost("tasks/{taskId:guid}/share-task")]
    public async Task<IActionResult> CreateUserTaskAccessAsync([FromRoute] Guid taskId, [FromBody] AccessEmailRequest request)
    {
        var command = new CreateUserTaskAccessCommand(taskId, CurrentUserId, request.Email);
        var result = await this._createUserTaskAccessHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        if (!result.IsSuccess)
        {
            return this.HandleResult(result);
        }

        return this.Ok(new { TaskId = taskId, request.Email });
    }

    /// <summary>
    /// Deletes a task access entry based on the user's email.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="email">The email of the user whose access should be removed.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the deletion.</returns>
    [HttpDelete("tasks/{taskId:guid}/by-email")]
    public async Task<IActionResult> DeleteTaskAccessByEmailAsync([FromRoute] Guid taskId, [FromQuery] string? email)
    {
        var command = new DeleteTaskAccessByUserEmailCommand(taskId, CurrentUserId, email);
        var result = await this._deleteTaskAccessByUserEmailHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Deletes a task access entry access ID.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="userId">The unique identifier of the user whose access will be removed.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the deletion.</returns>
    [HttpDelete("tasks/{taskId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> DeleteTaskAccessByIdAsync([FromRoute] Guid taskId, [FromRoute] Guid userId)
    {
        var command = new DeleteTaskAccessByIdCommand(taskId, userId);
        var result = await this._deleteTaskAccessByIdHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Deletes all access entries for a specific task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the deletion.</returns>
    [HttpDelete("tasks/{taskId:guid}")]
    public async Task<IActionResult> DeleteTaskAccessesByTaskAsync([FromRoute] Guid taskId)
    {
        var command = new DeleteTaskAccessesByTaskCommand(taskId, CurrentUserId);
        var result = await this._deleteTaskAccessesByTaskHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Deletes all task access entries associated with the current user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the deletion.</returns>
    [HttpDelete("users/my")]
    public async Task<IActionResult> DeleteTasksAccessesByUserAsync()
    {
        var command = new DeleteTaskAccessesByUserCommand(CurrentUserId);
        var result = await this._deleteTaskAccessesByUserHandler.HandleAsync(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
