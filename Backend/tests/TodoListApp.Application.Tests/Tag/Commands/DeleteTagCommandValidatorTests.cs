using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tag.Commands.DeleteTag;

namespace TodoListApp.Application.Tests.Tag.Commands
{
    /// <summary>
    /// Unit tests for <see cref="DeleteTagCommandValidator"/>.
    /// Verifies validation rules for deleting a tag.
    /// </summary>
    public class DeleteTagCommandValidatorTests
    {
        /// <summary>
        /// Tests that validation passes when the user owns the tag.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Validate_ShouldPass_WhenUserOwnsTag()
        {
            var tagId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Tags.IsTagOwnerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(true);

            var validator = new DeleteTagCommandValidator(unitOfWorkMock.Object);
            var command = new DeleteTagCommand(tagId, userId);

            var result = await validator.ValidateAsync(command);

            Assert.True(result.IsValid);
        }

        /// <summary>
        /// Tests that validation fails when the user does not own the tag.
        /// Ensures the proper error message is returned.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
        [Fact]
        public async Task Validate_ShouldFail_WhenUserDoesNotOwnTag()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Tags.IsTagOwnerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(false);

            var validator = new DeleteTagCommandValidator(unitOfWorkMock.Object);
            var command = new DeleteTagCommand(Guid.NewGuid(), Guid.NewGuid());

            var result = await validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            var error = Assert.Single(result.Errors, e => e.PropertyName == "TagId");
            Assert.Equal("Tag not found or does not belong to the user.", error.ErrorMessage);
        }
    }
}
