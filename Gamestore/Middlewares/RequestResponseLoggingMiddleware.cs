using System.Diagnostics;

namespace Gamestore.WebAPI.Middlewares;

public class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger = logger;

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        context.Request.EnableBuffering();
        var requestBody = await new StreamReader(
            context.Request.Body, leaveOpen: true).ReadToEndAsync();
        context.Request.Body.Position = 0;

        _logger.LogInformation(
            "REQUEST IS STARTED: IP: {IP}, URL: {URL}, Request: {Request}",
            context.Connection.RemoteIpAddress,
            context.Request.Path,
            requestBody);

        var originalResponseBody = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            responseBody.Position = 0;
            var responseText = await new StreamReader(
                responseBody, leaveOpen: true).ReadToEndAsync();
            responseBody.Position = 0;

            _logger.LogInformation(
                "IP: {IP}, URL: {URL}, Status: {Status}, Time: {Elapsed}ms, Request: {Request}, Response: {Response}",
                context.Connection.RemoteIpAddress,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestBody,
                responseText);

            await responseBody.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }
}
