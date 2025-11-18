using BusinessLogicLayer.Models;

namespace BusinessLogicLayer.Services;

public interface IGameService
{
    Task CreateGameAsync(GameModel gameModel);
}