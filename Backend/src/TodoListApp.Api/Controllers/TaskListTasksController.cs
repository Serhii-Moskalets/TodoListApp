using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
