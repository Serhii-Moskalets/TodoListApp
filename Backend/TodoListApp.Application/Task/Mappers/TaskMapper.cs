using Riok.Mapperly.Abstractions;
using TodoListApp.Application.Task.Dtos;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Task.Mappers;

/// <summary>
/// Provides mapping methods to convert domain entities to their corresponding DTOs.
/// Uses Mapperly with <see cref="RequiredMappingStrategy.Target"/> to enforce mapping completeness.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TaskMapper
{
    /// <summary>
    /// Maps a <see cref="TaskEntity"/> to a <see cref="TaskDto"/>.
    /// </summary>
    /// <param name="entity">The task entity to map.</param>
    /// <returns>The mapped <see cref="TaskDto"/>.</returns>
    public static partial TaskDto Map(TaskEntity entity);

    /// <summary>
    /// Maps a <see cref="TagEntity"/> to a <see cref="TagDto"/>.
    /// </summary>
    /// <param name="entity">The tag entity to map.</param>
    /// <returns>The mapped <see cref="TagDto"/>.</returns>
    public static partial TagDto Map(TagEntity entity);

    /// <summary>
    /// Maps a <see cref="CommentEntity"/> to a <see cref="CommentDto"/>.
    /// </summary>
    /// <param name="entity">The comment entity to map.</param>
    /// <returns>The mapped <see cref="CommentDto"/>.</returns>
    public static partial CommentDto Map(CommentEntity entity);

    /// <summary>
    /// Maps a <see cref="UserEntity"/> to a <see cref="UserDto"/>.
    /// </summary>
    /// <param name="entity">The user entity to map.</param>
    /// <returns>The mapped <see cref="UserDto"/>.</returns>
    public static partial UserDto Map(UserEntity entity);
}
