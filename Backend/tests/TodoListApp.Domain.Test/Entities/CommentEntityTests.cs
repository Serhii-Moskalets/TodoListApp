using System.Threading.Tasks;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Test.Entities;

public class CommentEntityTests
{
    [Fact]
    public void Constructor_ShouldCreateComment_WhenValidData()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var text = "Hello";

        var comment = new CommentEntity(taskId, userId, text);

        Assert.Equal(taskId, comment.TaskId);
        Assert.Equal(userId, comment.UserId);
        Assert.Equal(text, comment.Text);
        Assert.True(comment.CreatedDate <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenTextInvalid(string? invalidText)
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => new CommentEntity(taskId, userId, invalidText!));
    }

    [Theory]
    [InlineData("   Hello    ")]
    [InlineData("   Hello")]
    [InlineData("Hello    ")]
    public void Constructor_ShouldTrimName(string text)
    {
        var normalText = "Hello";
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), text);
        Assert.Equal(normalText, comment.Text);
    }

    [Fact]
    public void Update_ShouldChangeText_WhenValid()
    {
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), "Old");
        comment.Update("New");
        Assert.Equal("New", comment.Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_ShouldThrow_WhenTextInvalid(string? invalidText)
    {
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), "Old");
        Assert.Throws<ArgumentException>(() => comment.Update(invalidText!));
    }

    [Theory]
    [InlineData("   New    ")]
    [InlineData("   New")]
    [InlineData("New    ")]
    public void Update_ShouldTrimName(string text)
    {
        var normalText = "New";
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), "Old");
        comment.Update(text);
        Assert.Equal(comment.Text, normalText);
    }
}
