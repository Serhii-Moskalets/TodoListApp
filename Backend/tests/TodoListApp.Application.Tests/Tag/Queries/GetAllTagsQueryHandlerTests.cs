using FluentValidation;
using FluentValidation.Results;
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
    /// <summary>
    /// Returns failure if validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<GetAllTagsQuery>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetAllTagsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();
        var handler = new GetAllTagsQueryHandler(uowMock.Object, validatorMock.Object);

        var query = new GetAllTagsQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Returns a list of tags when validation passes and tags exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTags_WhenValidationPasses()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var validatorMock = new Mock<IValidator<GetAllTagsQuery>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetAllTagsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var tagEntities = new List<TagEntity>
        {
            new("Tag1", userId),
            new("Tag2", userId),
        };

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(tagEntities);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);

        var handler = new GetAllTagsQueryHandler(uowMock.Object, validatorMock.Object);
        var query = new GetAllTagsQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

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

        var validatorMock = new Mock<IValidator<GetAllTagsQuery>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetAllTagsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync([]);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);

        var handler = new GetAllTagsQueryHandler(uowMock.Object, validatorMock.Object);
        var query = new GetAllTagsQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}
