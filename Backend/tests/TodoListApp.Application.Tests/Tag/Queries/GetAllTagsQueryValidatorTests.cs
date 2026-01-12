using FluentValidation.TestHelper;
using TodoListApp.Application.Tag.Queries.GetAllTags;

namespace TodoListApp.Application.Tests.Tag.Queries;

/// <summary>
/// Contains unit tests for <see cref="GetAllTagsQueryValidator"/>.
/// Ensures that the validator correctly enforces rules for retrieving all tags.
/// </summary>
public class GetAllTagsQueryValidatorTests
{
    private readonly GetAllTagsQueryValidator _validator = new();

    /// <summary>
    /// Should return a validation error when the UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var query = new GetAllTagsQuery(Guid.Empty);

        // Act
        var result = this._validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.UserId)
              .WithErrorMessage("User ID is required.");
    }

    /// <summary>
    /// Should not return any validation errors when the UserId is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        // Arrange
        var query = new GetAllTagsQuery(Guid.NewGuid());

        // Act
        var result = this._validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.UserId);
    }
}
