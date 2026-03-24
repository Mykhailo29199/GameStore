using System.Text.RegularExpressions;
using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Enums.FilterOptionsEnums;
using GameStore.Shared.Exceptions;
using GameStore.Shared.Extensions;
using GameStore.Shared.FileSystem;
using Microsoft.EntityFrameworkCore;
using CultureInfo = System.Globalization.CultureInfo;
using PublichDateOptions = GameStore.Shared.Enums.FilterOptionsEnums.PublichDateOptions;

namespace BusinessLogicLayer.Services;

public class GameService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper, IFileSystem fileSystem, GameFilterPipeline pipeline) : IGameService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly GameFilterPipeline _pipeline = pipeline;

    public async Task CreateGameAsync(GameModel gameModel)
    {
        gameModel.Key = await GenerateUniqueGameKeyAsync(gameModel.Key, gameModel.Name);

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

        _ = await _unitOfWork.PublisherRepository.GetByIdAsync(gameModel.Publisher)
            ?? throw new NotFoundException($"Publisher with ID '{gameModel.Publisher}' was not found.");

        GameEntity gameEntity = _mapper.Map<GameEntity>(gameModel);
        await _unitOfWork.GameRepository.AddAsync(gameEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<GameModel> GetGameByKeyAsync(string key)
    {
        var gameEntity = await _unitOfWork.GameRepository.GetGameByKeyAsync(key)
            ?? throw new NotFoundException($"Game with Key '{key}' was not found.");

        gameEntity.Views++;

        await _unitOfWork.GameRepository.UpdateAsync(gameEntity);
        await _unitOfWork.SaveAsync();

        var resultGameEntity = _mapper.Map<GameModel>(gameEntity);

        return resultGameEntity;
    }

    public async Task<GameModel> GetGameByIdAsync(Guid gameId)
    {
        var gameEntity = await _unitOfWork.GameRepository.GetByIdAsync(gameId);

        return gameEntity != null
            ? _mapper.Map<GameModel>(gameEntity)
            : throw new NotFoundException($"Game with Id '{gameId}' was not found.");
    }

    public async Task UpdateGameAsync(GameModel gameModel)
    {
        GameEntity existingGame = await _unitOfWork.GameRepository.GetFullGame(gameModel.Id)
            ?? throw new NotFoundException($"Game with Id `{gameModel.Id}` was not found.");

        existingGame.Key = await GenerateUniqueGameKeyAsync(gameModel.Key, gameModel.Name);
        var allGenres = await _unitOfWork.GenreRepository.GetAllAsync();

        if (gameModel.Genres.Any(genreId => !allGenres.Any(g => g.Id == genreId)))
        {
            throw new NotFoundException("One or more genres were not found.");
        }

        var allPlatforms = await _unitOfWork.PlatformRepository.GetAllAsync();

        if (!gameModel.Platforms.All(platformId => allPlatforms.Any(p => p.Id == platformId)))
        {
            throw new NotFoundException("One or more platforms were not found.");
        }

        _ = await _unitOfWork.PublisherRepository.GetByIdAsync(gameModel.Publisher)
        ?? throw new NotFoundException($"Publisher with Id `{gameModel.Publisher}` was not found.");

        _mapper.Map(gameModel, existingGame);
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

        return _mapper.Map<IList<GenreModel>>(genres);
    }

    public async Task<IList<PlatformModel>> GetPlatformsByGameKeyAsync(string key)
    {
        var platforms = await _unitOfWork.GameRepository.GetPlatformsByGameKeyAsync(key);

        return _mapper.Map<IList<PlatformModel>>(platforms);
    }

    public async Task<PublisherModel> GetPublisherByGameKeyAsync(string key)
    {
        var publisher = await _unitOfWork.GameRepository.GetPublisherByGameKeyAsync(key)
            ?? throw new NotFoundException($"Game with Key '{key}' was not found or has no publisher.");

        var resultModel = _mapper.Map<PublisherModel>(publisher);
        return resultModel;
    }

    public async Task<int> GetTotalGamesCountAsync()
    {
        var games = await _unitOfWork.GameRepository.GetAllAsync();

        return games.Count();
    }

    public List<string> GetPaginationOptions()
    {
        return Enum.GetValues(typeof(PaginationOptions))
            .Cast<PaginationOptions>()
            .Select(x => x.GetDescription())
            .ToList();
    }

    public List<string> GetSortingOptions()
    {
        return Enum.GetValues(typeof(SortingOptions))
            .Cast<SortingOptions>()
            .Select(x => x.GetDescription())
            .ToList();
    }

    public List<string> GetPublichDateOptions()
    {
        return Enum.GetValues(typeof(PublichDateOptions))
        .Cast<PublichDateOptions>()
        .Select(x => x.GetDescription())
        .ToList();
    }

    public async Task<FilteredGamesResponseModel> GetFilteredGamesAsync(GameFilterModel filter)
    {
        var query = _unitOfWork.GameRepository.GetQueryable();

        int totalCount = await query.CountAsync();

        query = _pipeline.Execute(query, filter);

        var games = await query.ToListAsync();

        return new FilteredGamesResponseModel
        {
            Games = _mapper.Map<List<GameModel>>(games),
            CurrentPage = filter.Page,
            TotalPages = filter.PageCount == (int)PaginationOptions.All
             ? 1
             : (int)Math.Ceiling(totalCount / (double)filter.PageCount),
        };
    }

    private async Task<string> GenerateUniqueGameKeyAsync(string gameKey, string gameName)
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
