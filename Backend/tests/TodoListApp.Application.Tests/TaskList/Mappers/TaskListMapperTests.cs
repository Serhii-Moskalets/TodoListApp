using TodoListApp.Application.TaskList.Mappers;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.TaskList.Mappers;

/// <summary>
/// Unit tests for <see cref="TaskListMapper"/>.
/// </summary>
public class TaskListMapperTests
{
    /// <summary>
    /// Verifies that <see cref="TaskListMapper.Map(TaskListEntity)"/> correctly maps
    /// properties from entity to DTO.
    /// </summary>
    [Fact]
    public void Map_EntityToDto_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TaskListEntity(userId, "My Awesome List");

        // Act
        var dto = TaskListMapper.Map(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.Title, dto.Title);
        Assert.Equal(entity.OwnerId, dto.OwnerId);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListMapper.Map(IReadOnlyCollection{TaskListEntity})"/>
    /// correctly maps a collection of entities.
    /// </summary>
    [Fact]
    public void Map_CollectionToDtoList_ShouldMapAllItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entities = new List<TaskListEntity>
    {
        new(userId, "List 1"),
        new(userId, "List 2"),
        new(userId, "List 3"),
    };

        // Act
        var result = TaskListMapper.Map(entities);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entities.Count, result.Count);

        var entityList = entities.ToList();
        var dtoList = result.ToList();

        for (int i = 0; i < entities.Count; i++)
        {
            Assert.Equal(entityList[i].Title, dtoList[i].Title);
            Assert.Equal(entityList[i].Id, dtoList[i].Id);
        }
    }

    /// <summary>
    /// Verifies that the mapper returns an empty collection when provided an empty source.
    /// </summary>
    [Fact]
    public void Map_EmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var entities = Array.Empty<TaskListEntity>();

        // Act
        var result = TaskListMapper.Map(entities);

        // Assert
        Assert.Empty(result);
    }
}
