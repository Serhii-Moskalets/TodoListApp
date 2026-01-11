using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="TagEntity"/> domain entity.
/// </summary>
public class TagEntityTests
{
    private const string Name = "Tag";

    /// <summary>
    /// Verifies that the constructor creates a tag
    /// when valid name and user ID are provided.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateTag_WhenValidData()
    {
        var userId = Guid.NewGuid();

        var tag = new TagEntity(Name, userId);
        Assert.Equal(Name, tag.Name);
        Assert.Equal(userId, tag.UserId);
    }

    /// <summary>
    /// Verifies that the constructor throws an <see cref="DomainException"/>
    /// when the tag name is invalid.
    /// </summary>
    /// <param name="invalidText">An invalid tag name.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenTextInvalid(string? invalidText)
    {
        var userId = Guid.NewGuid();

        Assert.Throws<DomainException>(() => new TagEntity(invalidText!, userId));
    }

    /// <summary>
    /// Verifies that the constructor trims whitespace
    /// from the tag name.
    /// </summary>
    /// <param name="name">A tag name containing leading or trailing whitespace.</param>
    [Theory]
    [InlineData("   Tag   ")]
    [InlineData("Tag   ")]
    [InlineData("   Tag")]
    public void Constructor_ShouldTrimName(string name)
    {
        var userId = Guid.NewGuid();
        var tag = new TagEntity(name, userId);
        Assert.Equal(Name, tag.Name);
    }

    /// <summary>
    /// Verifies that the tasks collection is initialized
    /// when the tag is created.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeTasksCollection()
    {
        var tag = new TagEntity(Name, Guid.NewGuid());
        Assert.NotNull(tag.Tasks);
        Assert.Empty(tag.Tasks);
    }
}
