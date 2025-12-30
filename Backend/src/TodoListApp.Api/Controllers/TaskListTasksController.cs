using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Provides HTTP endpoints for deleting overdue tasks.
/// </summary>
/// <remarks>
/// This controller acts as an API layer and delegates
/// all business logic to application command and query handlers.
/// </remarks>
[ApiController]
[Route("api/task-lists/{taskListId:guid}/tasks")]
public class TaskListTasksController : BaseController
{
    private readonly ICommandHandler<DeleteOverdueTasksCommand, bool> _deleteOverdueTasksHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskListTasksController"/> class.
    /// </summary>
    /// <param name="deleteOverdueTasksHandler">Handler to delete overdue tasks from a task list.</param>
    public TaskListTasksController(ICommandHandler<DeleteOverdueTasksCommand, bool> deleteOverdueTasksHandler)
    {
        this._deleteOverdueTasksHandler = deleteOverdueTasksHandler;
    }

    /// <summary>
    /// Deletes all overdue tasks in the specified task list.
    /// </summary>
    /// <param name="taskListId">The task list identifier.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpDelete("overdue")]
    public async Task<IActionResult> DeleteOverdueTasks([FromRoute] Guid taskListId)
    {
        var command = new DeleteOverdueTasksCommand(taskListId, CurrentUserId);
        var result = await this._deleteOverdueTasksHandler.Handle(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
