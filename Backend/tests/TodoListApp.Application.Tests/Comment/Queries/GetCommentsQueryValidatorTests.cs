using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Queries.GetComments;

namespace TodoListApp.Application.Tests.Comment.Queries;

/// <summary>
/// Unit tests for <see cref="GetCommentsQueryValidator"/>.
/// Ensures that the validator correctly enforces access rules for getting comments.
/// </summary>
public class GetCommentsQueryValidatorTests
{
    /// <summary>
    /// Tests that validation passes when the user is the owner of the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenUserIsOwner()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var query = new GetCommentsQuery(Guid.NewGuid(), Guid.NewGuid());

        unitOfWorkMock.Setup(u => u.Tasks.ExistsForUserAsync(query.TaskId, query.UserId, default))
                      .ReturnsAsync(true);
        unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(query.TaskId, query.UserId, default))
                      .ReturnsAsync(false);

        var validator = new GetCommentsQueryValidator(unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(query);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Tests that validation passes when the user has shared access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenUserHasSharedAccess()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var query = new GetCommentsQuery(Guid.NewGuid(), Guid.NewGuid());

        unitOfWorkMock.Setup(u => u.Tasks.ExistsForUserAsync(query.TaskId, query.UserId, default))
                      .ReturnsAsync(false);
        unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(query.TaskId, query.UserId, default))
                      .ReturnsAsync(true);

        var validator = new GetCommentsQueryValidator(unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(query);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Tests that validation fails when the user has no access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenUserHasNoAccess()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var query = new GetCommentsQuery(Guid.NewGuid(), Guid.NewGuid());

        unitOfWorkMock.Setup(u => u.Tasks.ExistsForUserAsync(query.TaskId, query.UserId, default))
                      .ReturnsAsync(false);
        unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(query.TaskId, query.UserId, default))
                      .ReturnsAsync(false);

        var validator = new GetCommentsQueryValidator(unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "TaskId");
        Assert.Equal("Task not found or does not belong to the user.", error.ErrorMessage);
    }
}