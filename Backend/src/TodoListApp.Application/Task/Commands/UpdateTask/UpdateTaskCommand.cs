using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;

namespace TodoListApp.Application.Task.Commands.UpdateTask;

/// <summary>
/// Command to update an existing task using a DTO.
/// </summary>
public record UpdateTaskCommand(UpdateTaskDto Dto)
    : ICommand;
