using System.Text.RegularExpressions;
using AutoMapper;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Enums.FilterOptionsEnums;
using GameStore.Shared.Exceptions;
using GameStore.Shared.Extensions;
using GameStore.Shared.FileSystem;
using Microsoft.EntityFrameworkCore;
using CultureInfo = System.Globalization.CultureInfo;
using PublishDateOptions = GameStore.Shared.Enums.FilterOptionsEnums.PublishDateOptions;

namespace BusinessLogicLayer.Services;

public class GameService(
    ISqlServerUnitOfWork unitOfWork,
    IMapper mapper,
    IFileSystem fileSystem,
    GameFilterPipeline pipeline,
    IProductCatalog productCatalog,
    ICategoryCatalog categoryCatalog,
    ISupplierCatalog supplierCatalog) : IGameService
{
    private readonly ISqlServerUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly GameFilterPipeline _pipeline = pipeline;

    public async Task CreateGameAsync(GameModel gameModel)
    {
        gameModel.Key = await GenerateUniqueGameKeyAsync(gameModel.Key, gameModel.Name);

        await ValidatePlatformsAsync(gameModel.Platforms);
        await EnsureGenresInGameStoreAsync(gameModel.Genres);
        var publisher = await EnsurePublisherInGameStoreAsync(gameModel.Publisher);

        ProductEntity gameEntity = _mapper.Map<ProductEntity>(gameModel);
        gameEntity.SupplierID = publisher.Id;

        await _unitOfWork.GameRepository.AddAsync(gameEntity);
        await _unitOfWork.SaveAsync();
    }

    public async Task<GameModel> GetGameByKeyAsync(string key)
    {
        await productCatalog.IncreaseViewsAsync(key);
        await _unitOfWork.SaveAsync();

        var gameEntity = await productCatalog.GetByKeyAsync(key)
            ?? throw new NotFoundException($"Game with Key '{key}' was not found.");

        return _mapper.Map<GameModel>(gameEntity);
    }

    public async Task<GameModel> GetGameByIdAsync(Guid gameId)
    {
        var gameEntity = await productCatalog.GetByIdAsync(gameId);

        return gameEntity != null
            ? _mapper.Map<GameModel>(gameEntity)
            : throw new NotFoundException($"Game with Id '{gameId}' was not found.");
    }

    /// <summary>
    /// A Northwind game is copied to the Game Store database and the copy is updated; Northwind data stays untouched.
    /// </summary>
    public async Task UpdateGameAsync(GameModel gameModel)
    {
        var currentGame = await productCatalog.GetByIdAsync(gameModel.Id)
            ?? throw new NotFoundException($"Game with Id `{gameModel.Id}` was not found.");

        await ValidatePlatformsAsync(gameModel.Platforms);

        var existingGame = await productCatalog.EnsureInGameStoreAsync(currentGame.Key);

        // The key maps a game to its Northwind product, so it cannot be changed for Northwind games.
        gameModel.Key = await productCatalog.HasNorthwindCounterpartAsync(existingGame)
            || string.Equals(gameModel.Key, existingGame.Key, StringComparison.Ordinal)
            ? existingGame.Key
            : await GenerateUniqueGameKeyAsync(gameModel.Key, gameModel.Name);

        await EnsureGenresInGameStoreAsync(gameModel.Genres);
        var publisher = await EnsurePublisherInGameStoreAsync(gameModel.Publisher);

        UpdateGenres(existingGame, gameModel.Genres);
        UpdatePlatforms(existingGame, gameModel.Platforms);

        existingGame.ProductName = gameModel.Name;
        existingGame.Key = gameModel.Key;
        existingGame.Description = gameModel.Description;
        existingGame.UnitPrice = gameModel.Price;
        existingGame.UnitsInStock = gameModel.UnitInStock;
        existingGame.Discount = gameModel.Discount;
        existingGame.SupplierID = publisher.Id;

        await _unitOfWork.SaveAsync();
        await productCatalog.SyncUnitsInStockAsync([existingGame]);
    }

    /// <summary>
    /// Games that exist in Northwind are soft-deleted in the Game Store database, so the Northwind original stays hidden.
    /// </summary>
    public async Task DeleteGameAsync(string key)
    {
        var gameEntity = await productCatalog.GetByKeyAsync(key)
            ?? throw new NotFoundException($"Game with Key '{key}' was not found.");

        var sqlGame = await _unitOfWork.GameRepository.GetGameByKeyAsync(key);

        if (sqlGame is null)
        {
            await productCatalog.MarkNorthwindProductDeletedAsync(gameEntity);
        }
        else if (await productCatalog.HasNorthwindCounterpartAsync(sqlGame))
        {
            sqlGame.IsDeleted = true;
        }
        else
        {
            await _unitOfWork.GameRepository.DeleteAsync(sqlGame);
        }

        await _unitOfWork.SaveAsync();
    }

    public async Task<string> GetGameFilePathAsync(string gameKey)
    {
        if (await productCatalog.GetByKeyAsync(gameKey) == null)
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
        var gameEntities = await productCatalog.GetAllAsync();
        return _mapper.Map<IList<GameModel>>(gameEntities);
    }

    public async Task<IList<GenreModel>> GetGenresByGameKeyAsync(string key)
    {
        var game = await productCatalog.GetByKeyAsync(key);

        var genres = game?.ProductCategories
            .Where(pc => pc.Category is not null)
            .Select(pc => pc.Category)
            .ToList() ?? [];

        return _mapper.Map<IList<GenreModel>>(genres);
    }

    public async Task<IList<PlatformModel>> GetPlatformsByGameKeyAsync(string key)
    {
        var platforms = await _unitOfWork.GameRepository.GetPlatformsByGameKeyAsync(key);

        return _mapper.Map<IList<PlatformModel>>(platforms);
    }

    public async Task<PublisherModel> GetPublisherByGameKeyAsync(string key)
    {
        var game = await productCatalog.GetByKeyAsync(key);

        var publisher = (game is null ? null : game.Supplier ?? await supplierCatalog.GetByIdAsync(game.SupplierID))
            ?? throw new NotFoundException($"Game with Key '{key}' was not found or has no publisher.");

        return _mapper.Map<PublisherModel>(publisher);
    }

    public async Task<int> GetTotalGamesCountAsync()
    {
        var games = await productCatalog.GetAllAsync();

        return games.Count;
    }

    public List<string> GetPaginationOptions()
    {
        return EnumExtensions.GetDescriptions<PaginationOptions>();
    }

    public List<string> GetSortingOptions()
    {
        return EnumExtensions.GetDescriptions<SortingOptions>();
    }

    public List<string> GetPublichDateOptions()
    {
        return EnumExtensions.GetDescriptions<PublishDateOptions>();
    }

    public async Task<FilteredGamesResponseModel> GetFilteredGamesAsync(GameFilterModel filter)
    {
        var sqlGames = _pipeline.Filter(_unitOfWork.GameRepository.GetQueryable().AsNoTracking(), filter);
        var sqlCount = await sqlGames.CountAsync();

        // Only the games that can reach the requested page are read from SQL; the page is cut after the merge.
        var sqlCandidates = await _pipeline.Sort(sqlGames, filter).Take(GetCandidateCount(filter)).ToListAsync();
        var northwindGames = await productCatalog.GetNorthwindOnlyAsync(filter);

        var merged = _pipeline.Sort(sqlCandidates.Concat(northwindGames).AsQueryable(), filter);
        var games = _pipeline.Paginate(merged, filter).ToList();
        var totalCount = sqlCount + northwindGames.Count;

        return new FilteredGamesResponseModel
        {
            Games = _mapper.Map<List<GameModel>>(games),
            CurrentPage = filter.Page,
            TotalPages = filter.PageSize == (int)PaginationOptions.All
             ? 1
             : (int)Math.Ceiling(totalCount / (double)filter.PageSize),
        };
    }

    private static int GetCandidateCount(GameFilterModel filter)
    {
        if (filter.PageSize == (int)PaginationOptions.All)
        {
            return int.MaxValue;
        }

        return (int)Math.Min((long)Math.Max(filter.Page, 1) * filter.PageSize, int.MaxValue);
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

        while (await productCatalog.KeyExistsAsync(gameKey))
        {
            gameKey = $"{baseKey}-{counter}";
            counter++;
        }

        return gameKey;
    }

    private async Task ValidatePlatformsAsync(IEnumerable<Guid> platformIds)
    {
        var allPlatforms = await _unitOfWork.PlatformRepository.GetAllAsync();

        foreach (var platformId in platformIds)
        {
            if (!allPlatforms.Any(p => p.Id == platformId))
            {
                throw new NotFoundException("Platform", platformId);
            }
        }
    }

    private async Task EnsureGenresInGameStoreAsync(IEnumerable<Guid> genreIds)
    {
        foreach (var genreId in genreIds)
        {
            _ = await categoryCatalog.GetByIdAsync(genreId)
                ?? throw new NotFoundException("Genre", genreId);

            await categoryCatalog.EnsureInGameStoreAsync(genreId);
        }
    }

    private async Task<SupplierEntity> EnsurePublisherInGameStoreAsync(Guid publisherId)
    {
        _ = await supplierCatalog.GetByIdAsync(publisherId)
            ?? throw new NotFoundException($"Publisher with ID '{publisherId}' was not found.");

        return await supplierCatalog.EnsureInGameStoreAsync(publisherId);
    }

    // The links are changed through the tracked game, so a copy of a Northwind game that is not saved yet works the same way.
    private static void UpdateGenres(ProductEntity game, List<Guid> genreIds)
    {
        foreach (var removed in game.ProductCategories.Where(pc => !genreIds.Contains(pc.CategoryId)).ToList())
        {
            game.ProductCategories.Remove(removed);
        }

        foreach (var genreId in genreIds.Distinct().Where(id => game.ProductCategories.All(pc => pc.CategoryId != id)).ToList())
        {
            game.ProductCategories.Add(new ProductCategoryEntity { ProductId = game.Id, CategoryId = genreId });
        }
    }

    private static void UpdatePlatforms(ProductEntity game, List<Guid> platformIds)
    {
        foreach (var removed in game.ProductPlatforms.Where(pp => !platformIds.Contains(pp.PlatformId)).ToList())
        {
            game.ProductPlatforms.Remove(removed);
        }

        foreach (var platformId in platformIds.Distinct().Where(id => game.ProductPlatforms.All(pp => pp.PlatformId != id)).ToList())
        {
            game.ProductPlatforms.Add(new ProductPlatformEntity { ProductId = game.Id, PlatformId = platformId });
        }
    }
}
