using FluentValidation.TestHelper;
using TodoListApp.Application.UserTaskAccess.Queries.GetTaskWithSharedUsers;

namespace TodoListApp.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Tests for <see cref="GetTaskWithSharedUsersQueryValidator"/>.
/// </summary>
public class GetTaskWithSharedUsersQueryValidatorTests
{
    private readonly GetTaskWithSharedUsersQueryValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        var query = new GetTaskWithSharedUsersQuery(Guid.Empty, Guid.NewGuid());
        var result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
              .WithErrorMessage("TaskId is required.");
    }

    /// <summary>
    /// Ensures validation fails when the user identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var query = new GetTaskWithSharedUsersQuery(Guid.NewGuid(), Guid.Empty);
        var result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("UserId is required.");
    }

    /// <summary>
    /// Ensures no validation errors are returned when all query fields are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var query = new GetTaskWithSharedUsersQuery(Guid.NewGuid(), Guid.NewGuid());
        var result = this._validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
