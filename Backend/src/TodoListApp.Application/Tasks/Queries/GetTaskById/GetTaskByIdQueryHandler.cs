using FluentValidation;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Mappers;

namespace TodoListApp.Application.Tasks.Queries.GetTaskById;

/// <summary>
/// Handles the <see cref="GetTaskByIdQuery"/> to retrieve a specific task for a given user.
/// </summary>
public class GetTaskByIdQueryHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetTaskByIdQuery> validator)
    : HandlerBase(unitOfWork), IQueryHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly IValidator<GetTaskByIdQuery> _validator = validator;

    /// <summary>
    /// Handles the retrieval of a task and maps it to a <see cref="TaskDto"/>.
    /// </summary>
    /// <param name="query">The query containing the task and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{TaskDto}"/> representing the outcome of the operation:
    /// - Success with the <see cref="TaskDto"/> if the task exists
    /// - Failure with <see cref="ErrorCode.NotFound"/> if the task does not exist.
    /// </returns>
    public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(this._validator, query);
        if (!validation.IsSuccess)
        {
            return await Result<TaskDto>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var taskEntity = await this.UnitOfWork.Tasks.GetTaskByIdForUserAsync(query.TaskId, query.UserId, cancellationToken: cancellationToken);

        if (taskEntity is null)
        {
            return await Result<TaskDto>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        var taskDto = TaskMapper.Map(taskEntity);

        return await Result<TaskDto>.SuccessAsync(taskDto);
    }
}
