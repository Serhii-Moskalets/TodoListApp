using FluentValidation.TestHelper;
using TodoListApp.Application.Comment.Queries.GetComments;

namespace TodoListApp.Application.Tests.Comment.Queries;

/// <summary>
/// Unit tests for <see cref="GetCommentsQueryValidator"/>.
/// Ensures that the validator correctly enforces rules for retrieving comments.
/// </summary>
public class GetCommentsQueryValidatorTests
{
    private readonly GetCommentsQueryValidator _validator = new();

    /// <summary>
    /// Fails validation when UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var query = new GetCommentsQuery(Guid.NewGuid(), Guid.Empty);

        var result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.UserId)
              .WithErrorMessage("User ID is required.");
    }

    /// <summary>
    /// Fails validation when TaskId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        var query = new GetCommentsQuery(Guid.Empty, Guid.NewGuid());

        var result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.TaskId)
              .WithErrorMessage("Task ID is required.");
    }

    /// <summary>
    /// Passes validation when the query is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Query_Is_Valid()
    {
        var query = new GetCommentsQuery(Guid.NewGuid(), Guid.NewGuid());

        var result = this._validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}