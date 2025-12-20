using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Tag.Dtos;
using TodoListApp.Application.Tag.Mappers;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tag.Queries.GetAllTags;

/// <summary>
/// Handles the <see cref="GetAllTagsQuery"/> to retrieve all tags for a specific user.
/// </summary>
public class GetAllTagsQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IQueryHandler<GetAllTagsQuery, IEnumerable<TagDto>>
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
        var tagEntities = await this.UnitOfWork.Tags.GetByUserIdAsync(query.UserId, cancellationToken);

        var tagDtoList = TagMapper.Map(tagEntities);

        return await Result<IEnumerable<TagDto>>.SuccessAsync(tagDtoList);
    }
}
