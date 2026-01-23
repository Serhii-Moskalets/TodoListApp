using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Common.Extensions;
using TodoListApp.Application.Tag.Mappers;

namespace TodoListApp.Application.Tag.Queries.GetTags;

/// <summary>
/// Handles the <see cref="GetTagsQuery"/> to retrieve all tags for a specific user.
/// </summary>
public class GetTagsQueryHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetTagsQuery, Result<PagedResultDto<TagDto>>>
{
    /// <summary>
    /// Handles the query to get all tags for the specified user.
    /// </summary>
    /// <param name="query">The query containing the user's ID.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a list of <see cref="TagDto"/> objects for the user.
    /// </returns>
    public async Task<Result<PagedResultDto<TagDto>>> Handle(GetTagsQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await this.UnitOfWork.Tags
            .GetTagsAsync(
            query.UserId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var result = items.ToPagedResult(totalCount, query.Page, query.PageSize, TagMapper.Map);

        return await Result<PagedResultDto<TagDto>>.SuccessAsync(result);
    }
}
