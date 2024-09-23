using System.Net;
using System.Text.Json;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Helper;

/// <summary>
/// Middleware for handling exceptions globally.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ErrorHandlingMiddleware"/> class.
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="logger">The logger instance to log exceptions.</param>
public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{

    /// <summary>
    /// Invokes the middleware to handle the HTTP request and catch any exceptions.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the completion of request handling.</returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context); // Proceed to the next middleware
        }
        catch (AuthenticationException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message)) Console.WriteLine(ex.Message);
            // Log the exception details
            logger.LogError(ex, "An unexpected error occurred.");
            // Handle the exception
            await HandleExceptionAsync(context, ex);
        }
        catch (RequestException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message)) Console.WriteLine(ex.Message);
            // Log the exception details
            logger.LogError(ex, "An unexpected error occurred.");
            // Handle the exception
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message)) Console.WriteLine(ex.Message);
            // Log the exception details
            logger.LogError(ex, "An unexpected error occurred.");
            // Handle the exception
            throw;
        }
    }

    /// <summary>
    /// Handles the exception and returns a JSON-formatted error response.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="exception">The caught exception.</param>
    /// <returns>A task that writes the error response to the client.</returns>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Set status code depending on the exception type
        context.Response.StatusCode = (int) HttpStatusCode.OK;

        Model_Result response = new(ResultType.GeneralError);
        if (exception is AuthenticationException authenticationException)
        {
            response = new Model_Result(authenticationException.ErrorCode);
        }
        else if (exception is RequestException requestException)
        {
            response = new Model_Result(requestException.ErrorCode);
        }

        // Serialize the response to JSON
        string result = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(result);
    }
}
