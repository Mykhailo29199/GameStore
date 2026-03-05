using System.Text.RegularExpressions;
using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.FilterOptions;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;
using GameStore.Shared.FileSystem;
using Microsoft.EntityFrameworkCore;
using CultureInfo = System.Globalization.CultureInfo;

namespace BusinessLogicLayer.Services;

public class GameService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper, IFileSystem fileSystem) : IGameService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IFileSystem _fileSystem = fileSystem;

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
        GameEntity existingGame = await _unitOfWork.GameRepository.GetFull(gameModel.Id)
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

    public List<PaginationOptions> GetPaginationOptions()
    {
        return new List<PaginationOptions>()
        {
            new PaginationOptions { Pagination = "10" },
            new PaginationOptions { Pagination = "20" },
            new PaginationOptions { Pagination = "50" },
            new PaginationOptions { Pagination = "100" },
            new PaginationOptions { Pagination = "All" },
        };
    }

    public List<SortingOptions> GetSortingOptions()
    {
        return new List<SortingOptions>()
        {
            new SortingOptions { Sorting = "Most popular" },
            new SortingOptions { Sorting = "Most commented" },
            new SortingOptions { Sorting = "Price ASC" },
            new SortingOptions { Sorting = "Price DESC" },
            new SortingOptions { Sorting = "New" },
        };
    }

    public List<PublichDateOptions> GetPublichDateOptions()
    {
        return new List<PublichDateOptions>()
        {
            new PublichDateOptions { PublishDate = "Last 24 hours" },
            new PublichDateOptions { PublishDate = "Last week" },
            new PublichDateOptions { PublishDate = "Last month" },
            new PublichDateOptions { PublishDate = "Last year" },
            new PublichDateOptions { PublishDate = "All time" },
        };
    }

    public async Task<FilteredGamesResultModel> GetFilteredGamesAsync(GameFilterModel filter)
    {
        var query = _unitOfWork.GameRepository.GetQueryable();

        // 🔹 GENRES
        if (filter.Genres?.Count > 0)
        {
            query = query.Where(g =>
                g.GameGenres.Any(gg => filter.Genres.Contains(gg.GenreId)));
        }

        // 🔹 PLATFORMS
        if (filter.Platforms.Count > 0)
        {
            query = query.Where(g =>
                g.GamePlatforms.Any(gp => filter.Platforms.Contains(gp.PlatformId)));
        }

        // 🔹 PUBLISHERS
        if (filter.Publishers?.Count > 0)
        {
            query = query.Where(g => filter.Publishers.Contains(g.PublisherId));
        }

        // 🔹 PRICE
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(g => g.Price >= filter.MinPrice);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(g => g.Price <= filter.MaxPrice);
        }

        // 🔹 NAME (min 3 chars)
        if (!string.IsNullOrWhiteSpace(filter.Name) && filter.Name.Length >= 3)
        {
            query = query.Where(g => g.Name.Contains(filter.Name));
        }

        // 🔹 DATE
        if (!string.IsNullOrWhiteSpace(filter.DatePublishing))
        {
            var now = DateTime.UtcNow;
            DateTime fromDate = filter.DatePublishing switch
            {
                "Last week" => now.AddDays(-7),
                "Last month" => now.AddMonths(-1),
                "Last year" => now.AddYears(-1),
                "2 years" => now.AddYears(-2),
                "3 years" => now.AddYears(-3),
                _ => DateTime.MinValue,
            };

            query = query.Where(g => g.CreatedAt >= fromDate);
        }

        // 🔹 SORTING
        query = filter.Sort switch
        {
            "Most popular" => query.OrderByDescending(g => g.Views),
            "Most commented" => query.OrderByDescending(g => g.Comments.Count),
            "Price ASC" => query.OrderBy(g => g.Price),
            "Price DESC" => query.OrderByDescending(g => g.Price),
            "New" => query.OrderByDescending(g => g.CreatedAt),
            _ => query.OrderBy(g => g.Name),
        };

        int totalCount = await query.CountAsync();

        if (filter.PageCount != "All")
        {
            int pageSize = int.Parse(filter.PageCount);
            query = query.Skip((filter.Page - 1) * pageSize).Take(pageSize);
        }

        // var games = await query.ToListAsync();

        return new FilteredGamesResultModel
        {
            Games = _mapper.Map<List<GameModel>>(query),
            CurrentPage = filter.Page,
            TotalPages = filter.PageCount == "All"
                ? 1
                : (int)Math.Ceiling(totalCount / (double)int.Parse(filter.PageCount)),
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
