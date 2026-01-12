using FluentValidation.TestHelper;
using TodoListApp.Application.TaskList.Queries.GetAllTaskList;

namespace TodoListApp.Application.Tests.TaskList.Queries;

/// <summary>
/// Unit tests for <see cref="GetAllTaskListQueryValidator"/>.
/// Ensures that the validator correctly enforces rules for retrieving all task lists.
/// </summary>
public class GetAllTaskListQueryValidatorTests
{
    private readonly GetAllTaskListQueryValidator _validator = new();

    /// <summary>
    /// Validation fails when <see cref="GetAllTaskListQuery.UserId"/> is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var query = new GetAllTaskListQuery(Guid.Empty);

        // Act & Assert
        var result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.UserId)
              .WithErrorMessage("User ID is required.");
    }

    /// <summary>
    /// Validation succeeds when <see cref="GetAllTaskListQuery.UserId"/> is provided.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Provided()
    {
        // Arrange
        var query = new GetAllTaskListQuery(Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(q => q.UserId);
    }
}
