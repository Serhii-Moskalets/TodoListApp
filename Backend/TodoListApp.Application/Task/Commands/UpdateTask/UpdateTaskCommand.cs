using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;

namespace TodoListApp.Application.Task.Commands.UpdateTask;

/// <summary>
/// Represents a command to update an existing task.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UpdateTaskCommand"/> class with the specified task data.
/// </remarks>
/// <param name="dto">The data transfer object containing the details of the task to update.</param>
public class UpdateTaskCommand(UpdateTaskDto dto)
    : ICommand
{
    /// <summary>
    /// Gets the data transfer object containing the updated task details.
    /// </summary>
    public UpdateTaskDto Dto { get; } = dto;
}
