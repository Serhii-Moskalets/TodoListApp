using FluentValidation;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Comment.Queries.GetComments;

/// <summary>
/// Validator for <see cref="GetCommentsQuery"/>.
/// Ensures that the requesting user has access to the specified task,
/// either as the owner or via shared access.
/// </summary>
public class GetCommentsQueryValidator : AbstractValidator<GetCommentsQuery>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCommentsQueryValidator"/> class.
    /// </summary>
    /// <param name="unitOfWork">
    /// The unit of work used to access tasks and user-task access data.
    /// </param>
    public GetCommentsQueryValidator(IUnitOfWork unitOfWork)
    {
        this._unitOfWork = unitOfWork;

        this.RuleFor(x => x.TaskId)
            .MustAsync(this.CanUserAccessTaskAsync)
            .WithMessage("Task not found or does not belong to the user.");
    }

    /// <summary>
    /// Checks whether the user has access to the specified task.
    /// </summary>
    /// <param name="query">The query containing the task ID and the requesting user ID.</param>
    /// <param name="taskId">The ID of the task to validate access for.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// True if the user is the owner of the task or has shared access; otherwise, false.
    /// </returns>
    public async Task<bool> CanUserAccessTaskAsync(GetCommentsQuery query, Guid taskId, CancellationToken ct)
    {
        var isOwner = await this._unitOfWork.Tasks.ExistsForUserAsync(taskId, query.UserId, ct);
        var haveShared = await this._unitOfWork.UserTaskAccesses.ExistsAsync(taskId, query.UserId, ct);

        return isOwner || haveShared;
    }
}
