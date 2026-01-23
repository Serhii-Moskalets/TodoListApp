using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;

namespace TodoListApp.Application.Tag.Queries.GetTags;

/// <summary>
/// Query to retrieve all tags for a specific user.
/// </summary>
public record GetTagsQuery(Guid UserId, int Page = 1, int PageSize = 10)
    : IQuery<PagedResultDto<TagDto>>;
