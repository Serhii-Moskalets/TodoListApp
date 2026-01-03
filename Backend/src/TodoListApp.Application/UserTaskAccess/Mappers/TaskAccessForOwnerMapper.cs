using Riok.Mapperly.Abstractions;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.UserTaskAccess.Dtos;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.UserTaskAccess.Mappers;

/// <summary>
/// Provides mapping methods for converting <see cref="UserTaskAccessEntity"/> instances
/// to Data Transfer Objects (DTOs) used for task access management.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TaskAccessForOwnerMapper
{
    /// <summary>
    /// Maps a single <see cref="UserTaskAccessEntity"/> to a <see cref="UserBriefDto"/>.
    /// </summary>
    /// <param name="entity">The user-task access entity to map.</param>
    /// <returns>A <see cref="UserBriefDto"/> representing the user's access information.</returns>
    [MapProperty(nameof(UserTaskAccessEntity.User.Id), nameof(UserBriefDto.Id))]
    [MapProperty(nameof(UserTaskAccessEntity.User.UserName), nameof(UserBriefDto.UserName))]
    [MapProperty(nameof(UserTaskAccessEntity.User.Email), nameof(UserBriefDto.Email))]
    public static partial UserBriefDto Map(UserTaskAccessEntity entity);

    /// <summary>
    /// Maps a collection of <see cref="UserTaskAccessEntity"/> instances
    /// to a list of <see cref="UserBriefDto"/> objects.
    /// </summary>
    /// <param name="entities">The collection of user-task access entities.</param>
    /// <returns>A list of <see cref="UserBriefDto"/> objects.</returns>
    public static partial IList<UserBriefDto> Map(IReadOnlyCollection<UserTaskAccessEntity> entities);

    /// <summary>
    /// Maps a collection of <see cref="UserTaskAccessEntity"/> to a <see cref="TaskAccessListDto"/>,
    /// including the task details and the list of users who have access.
    /// </summary>
    /// <param name="entities">The collection of user-task access entities for a task.</param>
    /// <returns>A <see cref="TaskAccessListDto"/> containing task information and associated users.</returns>
    public static TaskAccessListDto MapToTaskAccess(IReadOnlyCollection<UserTaskAccessEntity> entities)
        => new ()
        {
            Id = entities.FirstOrDefault()?.TaskId ?? Guid.Empty,
            Title = entities.FirstOrDefault()?.Task.Title ?? string.Empty,
            Users = Map(entities) ?? [],
        };
}
