using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;

namespace TodoListApp.Application.Task.Commands.CreateTask;

/// <summary>
/// Command to create a new task using a DTO.
/// </summary>
public record CreateTaskCommand(CreateTaskDto Dto)
    : ICommand;
