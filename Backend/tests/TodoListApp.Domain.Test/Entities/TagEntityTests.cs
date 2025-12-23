using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Test.Entities;

public class TagEntityTests
{
    [Fact]
    public void Constructor_ShouldCreateTag_WhenValidData()
    {
        var name = "Tag";
        var userId = Guid.NewGuid();

        var tag = new TagEntity(name, userId);
        Assert.Equal(name, tag.Name);
        Assert.Equal(userId, tag.UserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenTextInvalid(string? invalidText)
    {
        var userId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => new TagEntity(invalidText!, userId));
    }

    [Theory]
    [InlineData("   Tag   ")]
    [InlineData("Tag   ")]
    [InlineData("   Tag")]
    public void Constructor_ShouldTrimName(string name)
    {
        var userId = Guid.NewGuid();
        var tag = new TagEntity(name, userId);
        Assert.Equal("Tag", tag.Name);
    }

    [Fact]
    public void Constructor_ShouldInitializeTasksCollection()
    {
        var tag = new TagEntity("Tag", Guid.NewGuid());
        Assert.NotNull(tag.Tasks);
        Assert.Empty(tag.Tasks);
    }
}
