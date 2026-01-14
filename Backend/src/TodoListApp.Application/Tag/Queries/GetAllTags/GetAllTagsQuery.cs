using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;

namespace TodoListApp.Application.Tag.Queries.GetAllTags;

/// <summary>
/// Query to retrieve all tags for a specific user.
/// </summary>
public record GetAllTagsQuery(Guid UserId) : IQuery<IEnumerable<TagDto>>;
