using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Tasks.Dtos;

namespace TodoListApp.Application.Tasks.Commands.CreateTask;

/// <summary>
/// Command to create a new task using a DTO.
/// </summary>
public record CreateTaskCommand(CreateTaskDto Dto, Guid UserId)
    : ICommand<Guid>;
