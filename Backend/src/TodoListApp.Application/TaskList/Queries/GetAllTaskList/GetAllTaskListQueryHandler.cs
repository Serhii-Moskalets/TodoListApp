using FluentValidation;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.TaskList.Dtos;
using TodoListApp.Application.TaskList.Mappers;

namespace TodoListApp.Application.TaskList.Queries.GetAllTaskList;

/// <summary>
/// Handles the <see cref="GetAllTaskListQuery"/> by retrieving all task lists
/// for a specific user and mapping them to <see cref="TaskListDto"/> objects.
/// </summary>
public class GetAllTaskListQueryHandler(
    IUnitOfWork unitOfWork,
    IValidator<GetAllTaskListQuery> validator)
    : HandlerBase(unitOfWork), IQueryHandler<GetAllTaskListQuery, IEnumerable<TaskListDto>>
{
    private readonly IValidator<GetAllTaskListQuery> _validator = validator;

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
        var validation = await ValidateAsync(this._validator, query);
        if (!validation.IsSuccess)
        {
            return await Result<IEnumerable<TaskListDto>>.FailureAsync(validation.Error!.Code, validation.Error.Message);
        }

        var taskListEntities = await this.UnitOfWork.TaskLists.GetByUserIdAsync(query.UserId, cancellationToken);
        var taskListDtoList = TaskListMapper.Map(taskListEntities);
        return await Result<IEnumerable<TaskListDto>>.SuccessAsync(taskListDtoList);
    }
}
