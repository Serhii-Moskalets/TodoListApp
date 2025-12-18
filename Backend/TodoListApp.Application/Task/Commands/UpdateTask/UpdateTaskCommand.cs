using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;

namespace TodoListApp.Application.Task.Commands.UpdateTask;

/// <summary>
/// Represents a command to update an existing task.
/// </summary>
public class UpdateTaskCommand : ICommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskCommand"/> class with the specified task data.
    /// </summary>
    /// <param name="dto">The data transfer object containing the details of the task to update.</param>
    public UpdateTaskCommand(UpdateTaskDto dto)
    {
        this.Dto = dto;
    }

    /// <summary>
    /// Gets the data transfer object containing the updated task details.
    /// </summary>
    public UpdateTaskDto Dto { get; }
}
