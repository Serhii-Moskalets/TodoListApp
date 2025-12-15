using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;

namespace TodoListApp.Application.Task.Commands.CreateTask;

/// <summary>
/// Represents a command to create a new task.
/// </summary>
public class CreateTaskCommand : ICommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskCommand"/> class with the specified task data.
    /// </summary>
    /// <param name="dto">The data transfer object containing the details of the task to create.</param>
    public CreateTaskCommand(CreateTaskDto dto)
    {
        this.Dto = dto;
    }

    /// <summary>
    /// Gets the data transfer object containing the task details.
    /// </summary>
    public CreateTaskDto Dto { get; }
}
