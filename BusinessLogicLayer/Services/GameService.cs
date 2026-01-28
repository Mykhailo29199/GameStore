using System.Text.RegularExpressions;
using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;
using GameStore.Shared.FileSystem;
using CultureInfo = System.Globalization.CultureInfo;

namespace BusinessLogicLayer.Services;

public class GameService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper, IFileSystem fileSystem) : IGameService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IFileSystem _fileSystem = fileSystem;

    public async Task CreateGameAsync(FullGameModel gameModel)
    {
        var game = gameModel.Game;
        game.Key = await EnsureUniqueGameKeyAsync(game.Key, game.Name);

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
        Guid actualGameId = existingGame.GameId;

        existingGame = _mapper.Map<GameEntity>(gameModel);

        await _unitOfWork.GameGenreRepository.DeleteByGameIdAsync(actualGameId);

        var allGenres = await _unitOfWork.GenreRepository.GetAllAsync();

        List<GameGenreEntity> gameGenreEntities = [];

        foreach (var genreId in gameModel.Genres)
        {
            if (allGenres.Any(x => x.Id == genreId))
            {
                gameGenreEntities.Add(new GameGenreEntity
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

        await _unitOfWork.GameGenreRepository.AddRangeAsync(gameGenreEntities);

        await _unitOfWork.GamePlatformRepository.DeleteByGameIdAsync(actualGameId);

        var allPlatforms = await _unitOfWork.PlatformRepository.GetAllAsync();
        List<GamePlatformEntity> gamePlatformEntities = [];

        foreach (var platformId in gameModel.Platforms)
        {
            if (allPlatforms.Any(x => x.Id == platformId))
            {
                gamePlatformEntities.Add(new GamePlatformEntity
                {
                    GameId = actualGameId,
                    PlatformId = platformId,
                });
            }
            else
            {
                throw new NotFoundException($"Platform with Id `{platformId}` was not found.");
            }
        }

        await _unitOfWork.GamePlatformRepository.AddRangeAsync(gamePlatformEntities);

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

        var fileEntity = await _unitOfWork.GameFileRepository.GetGameFileByKeyAsync(gameKey)
            ?? throw new NotFoundException($"Game file for game with Key '{gameKey}' was not found.");

        var filePath = fileEntity?.FilePath;
        _ = filePath ?? throw new NotFoundException("Game file not found.");

        return !_fileSystem.FileExists(filePath)
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

        return genres.Count == 0
            ? throw new NotFoundException($"Game with Key '{key}' was not found or has no genres.")
            : _mapper.Map<IList<GenreModel>>(genres);
    }

    public async Task<IList<PlatformModel>> GetPlatformsByGameKeyAsync(string key)
    {
        var platforms = await _unitOfWork.GameRepository.GetPlatformsByGameKeyAsync(key);

        return platforms.Count == 0
            ? throw new NotFoundException($"Game with Key '{key}' was not found or has no platforms.")
            : _mapper.Map<IList<PlatformModel>>(platforms);
    }

    public async Task<int> GetTotalGamesCountAsync()
    {
        var games = await _unitOfWork.GameRepository.GetAllAsync();

        return games.Count();
    }

    private async Task<string> EnsureUniqueGameKeyAsync(string gameKey, string gameName)
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
