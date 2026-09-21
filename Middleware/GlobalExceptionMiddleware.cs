using System.Net;
using System.Text.Json;
using HelpDesk.Api.Exceptions;

namespace HelpDesk.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "API error {StatusCode}: {Message}", ex.StatusCode, ex.Message);
            await WriteResponseAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            var message = _env.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred. Please try again later.";

            await WriteResponseAsync(context, StatusCodes.Status500InternalServerError, message);
        }
    }

    private static async Task WriteResponseAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = new
        {
            statusCode,
            message,
            timestamp = DateTime.UtcNow.ToString("o")
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}