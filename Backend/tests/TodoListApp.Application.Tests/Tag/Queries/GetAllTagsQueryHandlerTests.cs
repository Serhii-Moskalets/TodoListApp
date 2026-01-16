using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tag.Queries.GetAllTags;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tag.Queries;

/// <summary>
/// Unit tests for <see cref="GetAllTagsQueryHandler"/>.
/// Verifies validation and retrieval of tags for a user.
/// </summary>
public class GetAllTagsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITagRepository> _tagRepoMock;
    private readonly GetAllTagsQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllTagsQueryHandlerTests"/> class.
    /// </summary>
    public GetAllTagsQueryHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._tagRepoMock = new Mock<ITagRepository>();

        this._uowMock.Setup(u => u.Tags).Returns(this._tagRepoMock.Object);

        this._handler = new GetAllTagsQueryHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Returns a list of tags when validation passes and tags exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTags_WhenTagsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tagEntities = new List<TagEntity>
        {
            new("Tag1", userId),
            new("Tag2", userId),
        };

        this._tagRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(tagEntities);

        var query = new GetAllTagsQuery(userId);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Collection(
            result.Value,
            tag => Assert.Equal("Tag1", tag.Name),
            tag => Assert.Equal("Tag2", tag.Name));
    }

    /// <summary>
    /// Returns an empty list when the user has no tags.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTagsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        this._tagRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync([]);

        var query = new GetAllTagsQuery(userId);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}
