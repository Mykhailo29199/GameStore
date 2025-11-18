using AutoMapper;
using BusinessLogicLayer.Models;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using static GameStore.Shared.Exceptions.Exceptions;
using CultureInfo = System.Globalization.CultureInfo;

namespace BusinessLogicLayer.Services;

public class GameService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper) : IGameService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task CreateGameAsync(GameModel gameModel)
    {
        GameEntity gameEntity = _mapper.Map<GameEntity>(gameModel);

        gameEntity.GameKey = GenerateGameKeyIfMissing(gameEntity.GameKey, gameEntity.GameName);

        if (!await _unitOfWork.GameRepository.CheckIfKeyUniqueAsync(gameEntity.GameKey))
        {
            throw new GameKeyNotUniqueException(gameEntity.GameKey);
        }

        foreach (var platform in gameEntity.GamePlatforms)
        {
            if (!await _unitOfWork.PlatformRepository.CheckIfExistAsync(platform.PlatformId))
            {
                throw new PlatformNotFoundException(platform.PlatformId);
            }
        }

        foreach (var genre in gameEntity.GameGenres)
        {
            if (!await _unitOfWork.GenreRepository.CheckIfExistAsync(genre.GenreId))
            {
                throw new GenreNotFoundException(genre.GenreId);
            }
        }

        await _unitOfWork.GameRepository.AddAsync(gameEntity);
        await _unitOfWork.SaveAsync();
    }

    private static string GenerateGameKeyIfMissing(string gameKey, string gameName)
    {
        if (
            string.IsNullOrWhiteSpace(gameKey) || gameKey == string.Empty)
        {
            if (string.IsNullOrWhiteSpace(gameName) || gameName == string.Empty)
            {
                throw new GameNoNameException();
            }

            gameKey = gameName
                .ToLower(CultureInfo.CurrentCulture)
                .Replace(" ", "-")
                .Replace(".", string.Empty)
                .Replace(",", string.Empty)
                .Replace(":", string.Empty)
                .Replace(";", string.Empty)
                .Replace("!", string.Empty)
                .Replace("?", string.Empty);
        }

        return gameKey;
    }
}
