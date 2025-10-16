using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace DealershipInventorySystem.WebAPI.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case ArgumentNullException argumentNullException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Required parameter is missing";
                response.Detail = argumentNullException.ParamName;
                break;

            case ArgumentException argumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Invalid argument provided";
                response.Detail = argumentException.Message;
                break;

            case KeyNotFoundException keyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "Resource not found";
                response.Detail = keyNotFoundException.Message;
                break;

            case UnauthorizedAccessException unauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Access denied";
                response.Detail = unauthorizedAccessException.Message;
                break;

            case DbUpdateConcurrencyException concurrencyException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Message = "The record was modified by another user";
                response.Detail = concurrencyException.Message;
                break;

            case DbUpdateException dbUpdateException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Database operation failed";
                response.Detail = GetDbUpdateExceptionMessage(dbUpdateException);
                break;

            case InvalidOperationException invalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Invalid operation";
                response.Detail = invalidOperationException.Message;
                break;

            case TimeoutException timeoutException:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response.Message = "Request timed out";
                response.Detail = timeoutException.Message;
                break;

            case NotSupportedException notSupportedException:
                response.StatusCode = (int)HttpStatusCode.NotImplemented;
                response.Message = "Operation not supported";
                response.Detail = notSupportedException.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An internal server error occurred";
                response.Detail = "Please contact support if the problem persists";
                break;
        }

        context.Response.StatusCode = response.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private static string GetDbUpdateExceptionMessage(DbUpdateException exception)
    {
        if (exception.InnerException?.Message.Contains("duplicate key") == true ||
            exception.InnerException?.Message.Contains("UNIQUE constraint") == true)
        {
            return "A record with the same unique identifier already exists";
        }

        if (exception.InnerException?.Message.Contains("foreign key") == true ||
            exception.InnerException?.Message.Contains("FOREIGN KEY constraint") == true)
        {
            return "Cannot perform this operation due to related data constraints";
        }

        if (exception.InnerException?.Message.Contains("null value") == true ||
            exception.InnerException?.Message.Contains("NOT NULL constraint") == true)
        {
            return "Required field cannot be empty";
        }

        if (exception.InnerException?.Message.Contains("check constraint") == true)
        {
            return "The provided data violates business rules";
        }

        return "Database operation failed due to data validation error";
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
}