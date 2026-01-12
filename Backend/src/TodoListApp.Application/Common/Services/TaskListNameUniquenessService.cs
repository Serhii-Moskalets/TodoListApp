using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Common.Services;

/// <summary>
/// Provides functionality for generating a unique task list title for a specific user.
/// Ensures that the returned title does not conflict with existing task list titles
/// owned by the same user.
/// </summary>
public class TaskListNameUniquenessService : ITaskListNameUniquenessService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskListNameUniquenessService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to access repositories.</param>
    public TaskListNameUniquenessService(IUnitOfWork unitOfWork)
    {
        this._unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Generates a unique task list title for a user.
    /// </summary>
    /// <param name="userId">The identifier of the task list owner.</param>
    /// <param name="title">The base title to make unique.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A unique task list title.</returns>
    public async Task<string> GetUniqueNameAsync(Guid userId, string title, CancellationToken cancellationToken)
    {
        string newTitle = title;
        int suffix = 1;

        while (await this._unitOfWork.TaskLists.ExistsByTitleAsync(newTitle, userId, cancellationToken))
        {
            newTitle = $"{title} ({suffix++})";
        }

        return newTitle;
    }
}
