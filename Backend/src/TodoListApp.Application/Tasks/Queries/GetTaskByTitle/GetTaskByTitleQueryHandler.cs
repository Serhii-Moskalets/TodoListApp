using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Mappers;

namespace TodoListApp.Application.Tasks.Queries.GetTaskByTitle;

/// <summary>
/// Handles the <see cref="GetTaskByTitleQuery"/> to search for tasks by title for a specific user.
/// </summary>
public class GetTaskByTitleQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetTaskByTitleQuery, Result<PagedResultDto<TaskDto>>>
{
    /// <summary>
    /// Handles the retrieval of tasks that match the title search text and maps them to <see cref="TaskDto"/>.
    /// </summary>
    /// <param name="query">The query containing the user ID and title text to search for.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="TinyResult.Result{T}"/> representing the outcome of the operation.
    /// </returns>
    /// /// <remarks>
    /// If the search text is null, empty, or consists only of whitespace,
    /// an empty collection is returned.
    /// </remarks>
    public async Task<Result<PagedResultDto<TaskDto>>> Handle(GetTaskByTitleQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Text))
        {
            return await Result<PagedResultDto<TaskDto>>.SuccessAsync(new PagedResultDto<TaskDto>());
        }

        var (taskEntities, totalCount) = await this.UnitOfWork.Tasks
            .SearchByTitleAsync(query.UserId, query.Text, query.Page, query.PageSize, cancellationToken);
        var taskDtoList = TaskMapper.Map(taskEntities);

        var result = new PagedResultDto<TaskDto>
        {
            Items = taskDtoList,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };

        return await Result<PagedResultDto<TaskDto>>.SuccessAsync(result);
    }
}
