using BusinessLogicLayer.Models;

namespace BusinessLogicLayer.Services.Interaces;

public interface IGameService
{
    Task CreateGameAsync(GameModel gameModel);

    Task<GameModel> GetGameByKeyAsync(string key);

    Task<GameModel> GetGameByIdAsync(Guid gameId);

    Task UpdateGameAsync(GameModel gameModel);

    Task DeleteGameAsync(string key);

    Task<string> GetGameFilePathAsync(string gameKey);

    Task<IList<GameModel>> GetAllGamesAsync();

    Task<IList<GenreModel>> GetGenresByGameKeyAsync(string key);

    Task<IList<PlatformModel>> GetPlatformsByGameKeyAsync(string key);

    Task<PublisherModel> GetPublisherByGameKeyAsync(string key);

    Task<int> GetTotalGamesCountAsync();
}