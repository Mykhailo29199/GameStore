using BusinessLogicLayer.Models;

namespace BusinessLogicLayer.Services.Interaces;

public interface IGameService
{
    Task CreateGameAsync(FullGameModel gameModel);

    Task<GameModel> GetGameByKeyAsync(string key);

    Task<GameModel> GetGameByIdAsync(Guid gameId);

    Task UpdateGameAsync(FullGameModel gameModel);

    Task DeleteGameAsync(string key);

    Task<string> GetGameFilePathAsync(string gameKey);

    Task<IList<GameModel>> GetAllGamesAsync();

    Task<IList<GenreModel>> GetGenresByGameKeyAsync(string key);

    Task<IList<PlatformModel>> GetPlatformsByGameKeyAsync(string key);
}