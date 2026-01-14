using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tag.Mappers;

namespace TodoListApp.Application.Tag.Queries.GetAllTags;

/// <summary>
/// Handles the <see cref="GetAllTagsQuery"/> to retrieve all tags for a specific user.
/// </summary>
public class GetAllTagsQueryHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetAllTagsQuery, Result<IEnumerable<TagDto>>>
{
    /// <summary>
    /// Handles the query to get all tags for the specified user.
    /// </summary>
    /// <param name="query">The query containing the user's ID.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a list of <see cref="TagDto"/> objects for the user.
    /// </returns>
    public async Task<Result<IEnumerable<TagDto>>> Handle(GetAllTagsQuery query, CancellationToken cancellationToken)
    {
        var tagEntities = await this.UnitOfWork.Tags
            .GetByUserIdAsync(query.UserId, cancellationToken);

        return await Result<IEnumerable<TagDto>>.SuccessAsync(
            TagMapper.Map(tagEntities));
    }
}
