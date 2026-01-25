using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Api.Middleware;

/// <summary>
/// Provides a centralized exception handling mechanism for the HTTP request pipeline.
/// Captures unhandled exceptions, logs them, and returns consistent JSON responses to the client.
/// </summary>
/// <param name="next">The next middleware in the request pipeline.</param>
/// <param name="logger">The logger used for recording exception details and stack traces.</param>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>
    /// Invokes the middleware to process an HTTP request.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles the exception by mapping it to a specific HTTP status code and writing a JSON response.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> where the response will be written.</param>
    /// <param name="exception">The exception that occurred during request processing.</param>
    /// <returns>A <see cref="Task"/> that represents the completion of the response writing.</returns>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            DomainException => HttpStatusCode.BadRequest,                 // 400 - Business error
            KeyNotFoundException => HttpStatusCode.NotFound,              // 404 - Not Found
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,   // 401
            _ => HttpStatusCode.InternalServerError,                      // 500 - Generic server error
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            context.Response.StatusCode,
            Message = exception is DomainException
                ? exception.Message
                : "An unexpected error occurred. Please try again later.",
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
