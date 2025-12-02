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

    public async Task CreateGameAsync(FullGameModel gameModel)
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

    public async Task<GameModel> GetGameByKeyAsync(string key)
    {
        var gameEntity = await _unitOfWork.GameRepository.GetGameByKeyAsync(key);

        return gameEntity != null
            ? _mapper.Map<GameModel>(gameEntity)
            : throw new NotFoundException($"Game with Key '{key}' was not found.");
    }

    public async Task<GameModel> GetGameByIdAsync(Guid gameId)
    {
        var gameEntity = await _unitOfWork.GameRepository.GetByIdAsync(gameId);

        return gameEntity != null
            ? _mapper.Map<GameModel>(gameEntity)
            : throw new NotFoundException($"Game with Id '{gameId}' was not found.");
    }

    public async Task<IList<GameModel>> GetGamesByPlatformIdAsync(Guid platformId)
    {
        var gameEntities = await _unitOfWork.GameRepository.GetGamesByPlatformIdAsync(platformId);

        return (gameEntities != null && gameEntities.Count > 0)
            ? _mapper.Map<IList<GameModel>>(gameEntities)
            : throw new NotFoundException($"Game with Platform Id '{platformId}' was not found.");
    }

    public async Task<IList<GameModel>> GetGamesByGenreIdAsync(Guid genreId)
    {
        var gameEntities = await _unitOfWork.GameRepository.GetGamesByGenresIdAsync(genreId);

        return (gameEntities != null && gameEntities.Count > 0)
            ? _mapper.Map<IList<GameModel>>(gameEntities)
            : throw new NotFoundException($"Game with Genre Id '{genreId}' was not found.");
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
