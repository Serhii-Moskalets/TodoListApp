using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;

namespace TodoListApp.Application.Task.Commands.CreateTask;

/// <summary>
/// Represents a command to create a new task.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateTaskCommand"/> class with the specified task data.
/// </remarks>
/// <param name="dto">The data transfer object containing the details of the task to create.</param>
public class CreateTaskCommand(CreateTaskDto dto)
    : ICommand
{
    /// <summary>
    /// Gets the data transfer object containing the task details.
    /// </summary>
    public CreateTaskDto Dto { get; } = dto;
}
