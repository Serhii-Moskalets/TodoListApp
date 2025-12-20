using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Task.Dtos;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Task.Queries.GetTaskById;

/// <summary>
/// Handles the <see cref="GetTaskByIdQuery"/> to retrieve a specific task for a given user.
/// </summary>
public class GetTaskByIdQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IQueryHandler<GetTaskByIdQuery, TaskDto>
{
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
        var taskEntity = await this.UnitOfWork.Tasks.GetTaskByIdForUserAsync(query.TaskId, query.UserId, cancellationToken: cancellationToken);

        if (taskEntity is null)
        {
            return await Result<TaskDto>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        var commentDtoList = taskEntity.Comments
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Text = c.Text,
                CreatedDate = c.CreatedDate,
                User = new UserDto
                {
                    Id = c.User.Id,
                    UserName = c.User.UserName,
                },
            });

        var tagDto = taskEntity.Tag is not null
            ? new TagDto { Id = taskEntity.Tag.Id, Name = taskEntity.Tag.Name }
            : null;

        var taskDto = new TaskDto
        {
            Id = taskEntity.Id,
            Title = taskEntity.Title,
            Description = taskEntity.Description,
            CreatedDate = taskEntity.CreatedDate,
            DueDate = taskEntity.DueDate,
            Status = taskEntity.Status,
            OwnerId = taskEntity.OwnerId,
            TaskListId = taskEntity.TaskListId,
            Tag = tagDto,
            Comments = commentDtoList,
        };

        return await Result<TaskDto>.SuccessAsync(taskDto);
    }
}
