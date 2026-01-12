using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Tasks.Dtos;

namespace TodoListApp.Application.Tasks.Commands.UpdateTask;

/// <summary>
/// Command to update an existing task using a DTO.
/// </summary>
public record UpdateTaskCommand(UpdateTaskDto Dto, Guid UserId)
    : ICommand;
