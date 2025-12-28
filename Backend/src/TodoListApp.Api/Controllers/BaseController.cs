using System;
using Microsoft.AspNetCore.Mvc;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Base controller that provides common properties and methods
/// for API controllers, such as retrieving the current user's ID.
/// </summary>
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current user's ID.
    /// </summary>
    /// <returns>
    /// A <see cref="Guid"/> representing the currently authenticated user.
    /// In this example, a fixed GUID is returned as a placeholder.
    /// In a real application, this would be retrieved from the JWT token or authentication context.
    /// </returns>
    protected static Guid CurrentUserId => Guid.Parse("b3d5f0a1-6c3b-4a2e-9f1c-123456789abc");
}
