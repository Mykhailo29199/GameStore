namespace BusinessLogicLayer.Services.Interaces;

/// <summary>
/// Client of the external authorization microservice.
/// </summary>
public interface IExternalAuthClient
{
    Task<bool> AuthenticateAsync(string login, string password);
}
