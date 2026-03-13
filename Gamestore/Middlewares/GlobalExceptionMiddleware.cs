using System.Net;
using System.Text.Json;
using GameStore.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Middlewares;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await ProcessException(ex, (int)HttpStatusCode.NotFound, context);
        }
        catch (Exception ex)
        {
            await ProcessException(ex, (int)HttpStatusCode.InternalServerError, context);
        }
    }

    private async Task ProcessException(Exception ex, int statusCode, HttpContext context)
    {
        _logger.LogError(
                   ex,
                   "Unhandled exception. Type={Type}, Path={Path}",
                   ex.GetType().FullName,
                   context.Request.Path);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "An error occurred while processing your request.",
            Detail = ex.Message,
            Instance = context.Request.Path,
        };

        var result = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(result);
    }
}
