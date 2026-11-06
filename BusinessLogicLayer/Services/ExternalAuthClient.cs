using System.Net.Http.Json;
using BusinessLogicLayer.Services.Interaces;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class ExternalAuthClient(IHttpClientFactory httpClientFactory) : IExternalAuthClient
{
    public const string HttpClientName = "AuthService";

    private const string AuthUrl = "api/auth";

    public async Task<bool> AuthenticateAsync(string login, string password)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);

        try
        {
            var response = await client.PostAsJsonAsync(AuthUrl, new { email = login, password });

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            throw new BadRequestException("External authorization service is unavailable.");
        }
    }
}
