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
        game.Key = await GenerateGameKeyIfMissingAsync(game.Key, game.Key);

        if ((await _unitOfWork.GameRepository.GetGameByKeyAsync(game.Key)) != null)
        {
            throw new GameKeyNotUniqueException(game.Key);
        }

        var allPlatforms = await _unitOfWork.PlatformRepository.GetAllAsync();

        foreach (var platformId in gameModel.Platforms)
        {
            if (!allPlatforms.Any(p => p.Id == platformId))
            {
                throw new NotFoundException("Platform", platformId);
            }
        }

        var allGenres = await _unitOfWork.GenreRepository.GetAllAsync();

        foreach (var genreId in gameModel.Genres)
        {
            if (!allGenres.Any(g => g.Id == genreId))
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

    public async Task UpdateGameAsync(FullGameModel gameModel)
    {
        var gameId = gameModel.Game.Id;

        GameEntity existingGame = await _unitOfWork.GameRepository.GetByIdAsync(gameId) ?? throw new NotFoundException($"Game with Id `{gameId}` was not found.");
        var actualGameId = existingGame.GameId;

        existingGame = _mapper.Map<GameEntity>(gameModel);

        await _unitOfWork.GameGenreRepository.DeleteByGameIdAsync(actualGameId);
        foreach (var genreId in gameModel.Genres)
        {
            if (await _unitOfWork.GenreRepository.CheckIfExistAsync(genreId))
            {
                await _unitOfWork.GameGenreRepository.AddAsync(new GameGenreEntity
                {
                    GameId = actualGameId,
                    GenreId = genreId,
                });
            }
            else
            {
                throw new NotFoundException($"Genre with Id `{genreId}` was not found.");
            }
        }

        await _unitOfWork.GamePlatformRepository.DeleteByGameIdAsync(actualGameId);
        foreach (var platformId in gameModel.Platforms)
        {
            await _unitOfWork.GamePlatformRepository.AddAsync(new GamePlatformEntity
            {
                GameId = actualGameId,
                PlatformId = platformId,
            });
        }

        await _unitOfWork.GameRepository.UpdateAsync(existingGame);
        await _unitOfWork.SaveAsync();
    }

    public async Task DeleteGameAsync(string key)
    {
        var gameEntity = await _unitOfWork.GameRepository.GetGameByKeyAsync(key)
            ?? throw new NotFoundException($"Game with Key '{key}' was not found.");

        await _unitOfWork.GameRepository.DeleteAsync(gameEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<string> GetGameFilePathAsync(string gameKey)
    {
        if (await _unitOfWork.GameRepository.GetGameByKeyAsync(gameKey) == null)
        {
            throw new NotFoundException($"Game with Key '{gameKey}' was not found.");
        }

        var fileEntity = await _unitOfWork.GameFileRepository.GetGameFileByKeyAsync(gameKey);
        var filePath = fileEntity?.FilePath;

        _ = filePath ?? throw new NotFoundException("Game file not found.");

        return !File.Exists(filePath)
            ? throw new NotFoundException(
                $"Physical file with path '{filePath}' not found on server.")
            : filePath;
    }

    public async Task<IList<GameModel>> GetAllGamesAsync()
    {
        var gameEntities = await _unitOfWork.GameRepository.GetAllAsync();
        return _mapper.Map<IList<GameModel>>(gameEntities);
    }

    public async Task<IList<GenreModel>> GetGenresByGameKeyAsync(string key)
    {
        var genres = await _unitOfWork.GameRepository.GetGenresByGameKeyAsync(key);

        return genres == null || genres.Count == 0
            ? throw new NotFoundException($"Game with Key '{key}' was not found or has no genres.")
            : _mapper.Map<IList<GenreModel>>(genres);
    }

    public async Task<IList<PlatformModel>> GetPlatformsByGameKeyAsync(string key)
    {
        var platforms = await _unitOfWork.GameRepository.GetPlatformsByGameKeyAsync(key);

        return platforms == null || platforms.Count == 0
            ? throw new NotFoundException($"Game with Key '{key}' was not found or has no platforms.")
            : _mapper.Map<IList<PlatformModel>>(platforms);
    }

    private async Task<string> GenerateGameKeyIfMissingAsync(string gameKey, string gameName)
    {
        if (string.IsNullOrWhiteSpace(gameKey))
        {
            gameKey = gameName;
        }

        gameKey = gameKey.ToLower(CultureInfo.CurrentCulture).Replace(" ", "-");
        var pattern = @"[^\p{L}0-9\-]";
        gameKey = Regex.Replace(gameKey, pattern, string.Empty);

        string baseKey = gameKey;
        int counter = 1;

        while (await _unitOfWork.GameRepository.GetGameByKeyAsync(gameKey) != null)
        {
            gameKey = $"{baseKey}-{counter}";
            counter++;
        }

        return gameKey;
    }
}
