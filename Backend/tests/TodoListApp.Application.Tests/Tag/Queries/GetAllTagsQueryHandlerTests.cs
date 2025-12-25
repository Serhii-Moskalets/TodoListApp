using Moq;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tag.Queries.GetAllTags;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.Tag.Queries
{
    /// <summary>
    /// Unit tests for <see cref="GetAllTagsQueryHandler"/>.
    /// Verifies retrieval of tags for a specific user.
    /// </summary>
    public class GetAllTagsQueryHandlerTests
    {
        /// <summary>
        /// Ensures that the handler returns a list of <see cref="TagDto"/> for the user.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Handle_ShouldReturnTags_ForUser()
        {
            var userId = Guid.NewGuid();

            var tags = new List<TagEntity>
            {
                new("Tag1", userId),
                new("Tag2", userId),
            };

            var tagRepoMock = new Mock<ITagRepository>();
            tagRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(tags);

            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);

            var handler = new GetAllTagsQueryHandler(uowMock.Object);
            var query = new GetAllTagsQuery(userId);

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            var tagDtos = result.Value?.ToList();
            Assert.Equal(2, tagDtos?.Count);
            Assert.Contains(tagDtos!, t => t.Name == "Tag1");
            Assert.Contains(tagDtos!, t => t.Name == "Tag2");

            tagRepoMock.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Ensures that the handler returns an empty list when the user has no tags.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoTagsExist()
        {
            var userId = Guid.NewGuid();

            var tagRepoMock = new Mock<ITagRepository>();
            tagRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync([]);

            var uowMock = new Mock<IUnitOfWork>();
            uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);

            var handler = new GetAllTagsQueryHandler(uowMock.Object);
            var query = new GetAllTagsQuery(userId);

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value!);

            tagRepoMock.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
