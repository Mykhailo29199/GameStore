using BusinessLogicLayer.Models;

namespace BusinessLogicLayer.Services.Interaces;

public interface IGameService
{
    Task CreateGameAsync(GameModel gameModel);
}