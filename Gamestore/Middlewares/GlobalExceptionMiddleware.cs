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
            _logger.LogError(
                ex,
                ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";

            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "An error occurred while processing your request.",
                Detail = ex.Message,
                Instance = context.Request.Path,
            };

            var result = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                               ex,
                               "Unhandled exception. Type={Type}, Path={Path}",
                               ex.GetType().FullName,
                               context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An error occurred while processing your request.",
                Detail = ex.Message,
                Instance = context.Request.Path,
            };

            var result = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(result);
        }
    }
}
