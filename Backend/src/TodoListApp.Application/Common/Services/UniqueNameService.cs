using TodoListApp.Application.Abstractions.Interfaces.Services;

namespace TodoListApp.Application.Common.Services;

/// <summary>
/// Provides a generic implementation for generating unique names using a numeric suffix.
/// </summary>
public class UniqueNameService : IUniqueNameService
{
    /// <inheritdoc />
    /// <remarks>
    /// This implementation appends a numeric suffix in the format "BaseName (N)"
    /// starting from 1 until the <paramref name="existsCheck"/> returns false.
    /// </remarks>
    public async Task<string> GetUniqueNameAsync(
        string baseName,
        Func<string, CancellationToken, Task<bool>> existsCheck,
        CancellationToken cancellationToken)
    {
        string newName = baseName;
        int suffix = 1;

        while (await existsCheck(newName, cancellationToken))
        {
            newName = $"{baseName} ({suffix++})";
        }

        return newName;
    }
}
