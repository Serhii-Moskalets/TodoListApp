using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tag.Mappers;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tag.Mappers;

/// <summary>
/// Unit tests for <see cref="TagMapper"/>.
/// Verifies that the entity is correctly mapped to the DTO.
/// </summary>
public class TagMapperTests
{
    /// <summary>
    /// Tests that a single <see cref="TagEntity"/> maps correctly to <see cref="TagDto"/>.
    /// </summary>
    [Fact]
    public void Map_EntityToDto_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entity = new TagEntity("Work", userId);
        var tagId = Guid.NewGuid();
        typeof(TagEntity).GetProperty(nameof(TagEntity.Id))?.SetValue(entity, tagId);

        // Act
        var dto = TagMapper.Map(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(tagId, dto.Id);
        Assert.Equal("Work", dto.Name);
    }

    /// <summary>
    /// Tests that a collection of <see cref="TagEntity"/> maps correctly to a collection of <see cref="TagDto"/>.
    /// </summary>
    [Fact]
    public void Map_CollectionToDtoList_ShouldMapAllItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entities = new List<TagEntity>
        {
            new("Urgent", userId),
            new("Personal", userId),
            new("Study", userId),
        };

        // Act
        var dtos = TagMapper.Map(entities);

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(entities.Count, dtos.Count);
        Assert.Collection(
            dtos,
            item => Assert.Equal("Urgent", item.Name),
            item => Assert.Equal("Personal", item.Name),
            item => Assert.Equal("Study", item.Name));
    }

    /// <summary>
    /// Tests that an empty collection of entities maps to an empty collection of DTOs.
    /// </summary>
    [Fact]
    public void Map_EmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var entities = new List<TagEntity>();

        // Act
        var dtos = TagMapper.Map(entities);

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }
}
