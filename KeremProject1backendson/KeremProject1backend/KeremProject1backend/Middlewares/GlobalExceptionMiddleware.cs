using KeremProject1backend.Core.Constants;
using KeremProject1backend.Models.DTOs;
using System.Net;
using System.Text.Json;

namespace KeremProject1backend.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. Path: {Path}, Method: {Method}",
                context.Request.Path, context.Request.Method);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        BaseResponse<string> errorResponse;

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = BaseResponse<string>.ErrorResponse(
                    "Unauthorized: " + exception.Message,
                    ErrorCodes.Unauthorized);
                break;

            case ArgumentNullException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = BaseResponse<string>.ErrorResponse(
                    "Invalid request: " + exception.Message,
                    ErrorCodes.ValidationFailed);
                break;

            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = BaseResponse<string>.ErrorResponse(
                    "Invalid request: " + exception.Message,
                    ErrorCodes.ValidationFailed);
                break;

            case KeyNotFoundException:
            case FileNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = BaseResponse<string>.ErrorResponse(
                    "Resource not found: " + exception.Message,
                    ErrorCodes.GenericError);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = BaseResponse<string>.ErrorResponse(
                    "An error occurred while processing your request.",
                    ErrorCodes.GenericError);
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}

