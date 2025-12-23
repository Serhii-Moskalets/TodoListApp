using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.TaskList.Dtos;
using TodoListApp.Application.TaskList.Mappers;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.TaskList.Queries.GetAllTaskList;

/// <summary>
/// Handles the <see cref="GetAllTaskListQuery"/> by retrieving all task lists
/// for a specific user and mapping them to <see cref="TaskListDto"/> objects.
/// </summary>
public class GetAllTaskListQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IQueryHandler<GetAllTaskListQuery, IEnumerable<TaskListDto>>
{
    /// <summary>
    /// Processes the query to get all task lists for a given user.
    /// </summary>
    /// <param name="query">The query containing the user ID.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a collection of <see cref="TaskListDto"/> objects.
    /// </returns>
    public async Task<Result<IEnumerable<TaskListDto>>> Handle(GetAllTaskListQuery query, CancellationToken cancellationToken)
    {
        var taskListEntities = await this.UnitOfWork.TaskLists.GetByUserIdAsync(query.UserId, cancellationToken);

        var taskListDtoList = TaskListMapper.Map(taskListEntities);

        return await Result<IEnumerable<TaskListDto>>.SuccessAsync(taskListDtoList);
    }
}
