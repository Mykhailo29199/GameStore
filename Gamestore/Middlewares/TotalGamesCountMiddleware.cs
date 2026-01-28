using BusinessLogicLayer.Services.Interaces;
using Microsoft.Extensions.Caching.Memory;

namespace Gamestore.WebAPI.Middlewares;

public class TotalGamesCountMiddleware(RequestDelegate next, IMemoryCache cache)
{
    private const string CacheKey = "TotalGamesCount";
    private readonly RequestDelegate _next = next;
    private readonly IMemoryCache _cache = cache;

    public async Task InvokeAsync(HttpContext context, IGameService gameService)
    {
        if (!_cache.TryGetValue(CacheKey, out int totalGames))
        {
            totalGames = await gameService.GetTotalGamesCountAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

            _cache.Set(CacheKey, totalGames, cacheEntryOptions);
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["x-total-numbers-of-games"] = totalGames.ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }
}