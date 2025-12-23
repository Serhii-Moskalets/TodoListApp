using TodoListApp.Domain.Entities;

namespace TodoListApp.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="CommentEntity"/> domain entity.
/// </summary>
public class CommentEntityTests
{
    private const string Text = "Hello";

    /// <summary>
    /// Verifies that the constructor creates a comment
    /// when valid task ID, user ID, and text are provided.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateComment_WhenValidData()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var comment = new CommentEntity(taskId, userId, Text);

        Assert.Equal(taskId, comment.TaskId);
        Assert.Equal(userId, comment.UserId);
        Assert.Equal(Text, comment.Text);
        Assert.True(comment.CreatedDate <= DateTime.UtcNow);
    }

    /// <summary>
    /// Verifies that the constructor throws an <see cref="ArgumentException"/>
    /// when the comment text is null, empty, or whitespace.
    /// </summary>
    /// <param name="invalidText">An invalid comment text.</param>
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

    /// <summary>
    /// Verifies that the constructor trims whitespace
    /// from the comment text.
    /// </summary>
    /// <param name="text">A comment text containing leading or trailing whitespace.</param>
    [Theory]
    [InlineData("   Hello    ")]
    [InlineData("   Hello")]
    [InlineData("Hello    ")]
    public void Constructor_ShouldTrimName(string text)
    {
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), text);
        Assert.Equal(Text, comment.Text);
    }

    /// <summary>
    /// Verifies that the comment text is updated
    /// when a valid new text is provided.
    /// </summary>
    [Fact]
    public void Update_ShouldChangeText_WhenValid()
    {
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), "Old");
        comment.Update("New");
        Assert.Equal("New", comment.Text);
    }

    /// <summary>
    /// Verifies that <see cref="CommentEntity.Update"/>
    /// throws an <see cref="ArgumentException"/>
    /// when the new text is invalid.
    /// </summary>
    /// <param name="invalidText">An invalid new comment text.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_ShouldThrow_WhenTextInvalid(string? invalidText)
    {
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), "Old");
        Assert.Throws<ArgumentException>(() => comment.Update(invalidText!));
    }

    /// <summary>
    /// Verifies that <see cref="CommentEntity.Update"/>
    /// trims whitespace from the updated comment text.
    /// </summary>
    /// <param name="text">A new comment text containing leading or trailing whitespace.</param>
    [Theory]
    [InlineData("   New text   ")]
    [InlineData("   New text")]
    [InlineData("New text    ")]
    public void Update_ShouldTrimName(string text)
    {
        var normalText = "New text";
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), Text);
        comment.Update(text);
        Assert.Equal(comment.Text, normalText);
    }
}
