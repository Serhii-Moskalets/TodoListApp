using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;

namespace TodoListApp.Application.Comment.Queries.GetComments;

/// <summary>
/// Represents a query to retrieve all comments for a specific task.
/// </summary>
public record GetCommentsQuery(
    Guid TaskId,
    Guid UserId,
    int Page = 1,
    int PageSize = 10)
    : IQuery<PagedResultDto<CommentDto>>;