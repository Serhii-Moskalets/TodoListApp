using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Comment.Mappers;
using TodoListApp.Application.Common.Dtos;

namespace TodoListApp.Application.Comment.Queries.GetComments;

/// <summary>
/// Handles the <see cref="GetCommentsQuery"/> to retrieve comments for a specific task.
/// </summary>
public class GetCommentsQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IQueryHandler<GetCommentsQuery, IEnumerable<CommentDto>>
{
    /// <summary>
    /// Handles the specified <see cref="GetCommentsQuery"/>.
    /// </summary>
    /// <param name="query">The query containing the task ID to retrieve comments for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a collection of <see cref="CommentDto"/> objects.
    /// </returns>
    public async Task<Result<IEnumerable<CommentDto>>> Handle(GetCommentsQuery query, CancellationToken cancellationToken)
    {
        var commentEntityList = await this.UnitOfWork.Comments
            .GetByTaskIdAsync(query.TaskId, cancellationToken);

        return await Result<IEnumerable<CommentDto>>.SuccessAsync(
            CommentMapper.Map(commentEntityList));
    }
}