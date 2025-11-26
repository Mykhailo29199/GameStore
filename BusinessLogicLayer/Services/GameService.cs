using System.Text.RegularExpressions;
using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;
using CultureInfo = System.Globalization.CultureInfo;

namespace BusinessLogicLayer.Services;

public class GameService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper) : IGameService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task CreateGameAsync(GameModel gameModel)
    {
        var game = gameModel.Game;
        game.Key = GenerateGameKeyIfMissing(game.Key, game.Key);

        if ((await _unitOfWork.GameRepository.GetGameByKeyAsync(game.Key)) == null)
        {
            throw new GameKeyNotUniqueException(game.Key);
        }

        var allPlatforms = await _unitOfWork.PlatformRepository.GetAllAsync();

        foreach (var platformId in gameModel.Platforms)
        {
            if (!allPlatforms.Any(p => p.PlatformId == platformId))
            {
                throw new NotFoundException("Platform", platformId);
            }
        }

        var allGenres = await _unitOfWork.GenreRepository.GetAllAsync();

        foreach (var genreId in gameModel.Genres)
        {
            if (!allGenres.Any(g => g.GenreId == genreId))
            {
                throw new NotFoundException("Genre", genreId);
            }
        }

        GameEntity gameEntity = _mapper.Map<GameEntity>(gameModel);
        await _unitOfWork.GameRepository.AddAsync(gameEntity);
        await _unitOfWork.SaveAsync();
    }

    private static string GenerateGameKeyIfMissing(string gameKey, string gameName)
    {
        if (string.IsNullOrWhiteSpace(gameKey))
        {
            gameKey = gameName;
        }

        gameKey = gameKey.ToLower(CultureInfo.CurrentCulture).Replace(" ", "-");
        var pattern = @"[^\p{L}0-9\-]";
        gameKey = Regex.Replace(gameKey, pattern, string.Empty);

        return gameKey;
    }
}
