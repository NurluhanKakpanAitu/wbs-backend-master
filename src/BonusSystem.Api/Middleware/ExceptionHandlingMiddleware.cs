using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BonusSystem.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Business exception occurred for path: {Path}, user: {User}",
                context.Request.Path,
                context.User.Identity?.Name ?? "anonymous");
            await WriteErrorResponse(context, 400, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error for path: {Path}, user: {User}",
                context.Request.Path,
                context.User.Identity?.Name ?? "anonymous");
            await WriteErrorResponse(context, 500, ex.Message);

        }
    }
    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var result = new
        {
            message
        };

        await context.Response.WriteAsJsonAsync(result);
    }
}
