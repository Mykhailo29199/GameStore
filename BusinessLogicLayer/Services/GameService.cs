using System.Data;
using System.Text.RegularExpressions;
using AutoMapper;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Localization;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines;
using BusinessLogicLayer.Security;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Authorization;
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
    IStoreCatalog catalog,
    ICurrentUser currentUser,
    IContentLocalizer localizer) : IGameService
{
    private readonly ISqlServerUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly GameFilterPipeline _pipeline = pipeline;

    private bool CanSeeDeletedGames => currentUser.HasPermission(Permissions.ViewDeletedGames);

    public async Task CreateGameAsync(GameModel gameModel)
    {
        gameModel.Key = await GenerateUniqueGameKeyAsync(gameModel.Key, gameModel.Name);

        await ValidatePlatformsAsync(gameModel.Platforms);
        await EnsureGenresInGameStoreAsync(gameModel.Genres);
        var publisher = await EnsurePublisherInGameStoreAsync(gameModel.Publisher);

        ProductEntity gameEntity = _mapper.Map<ProductEntity>(gameModel);
        gameEntity.SupplierID = publisher.Id;

        // English data is required, so a game added in another language also gets it as the English version.
        if (!localizer.IsDefaultLanguage)
        {
            await localizer.SaveGameTranslationAsync(gameEntity, gameModel.Name, gameModel.Description);
        }

        await _unitOfWork.ProductRepository.AddAsync(gameEntity);
        await _unitOfWork.SaveAsync();
    }

    /// <summary>
    /// The game and its translation are read in one transaction, so they cannot be changed in between.
    /// </summary>
    public async Task<GameModel> GetGameByKeyAsync(string key)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(
            async () =>
            {
                await catalog.Products.IncreaseViewsAsync(key);
                await _unitOfWork.SaveAsync();

                var gameEntity = await catalog.Products.GetByKeyAsync(key, CanSeeDeletedGames)
                    ?? throw new NotFoundException($"Game with Key '{key}' was not found.");

                return await LocalizeAsync(_mapper.Map<GameModel>(gameEntity));
            },
            IsolationLevel.RepeatableRead);
    }

    public async Task<GameModel> GetGameByIdAsync(Guid gameId)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(
            async () =>
            {
                var gameEntity = await catalog.Products.GetByIdAsync(gameId, CanSeeDeletedGames)
                    ?? throw new NotFoundException($"Game with Id '{gameId}' was not found.");

                return await LocalizeAsync(_mapper.Map<GameModel>(gameEntity));
            },
            IsolationLevel.RepeatableRead);
    }

    /// <summary>
    /// A Northwind game is copied to the Game Store database and the copy is updated; Northwind data stays untouched.
    /// The name and description are saved in the language of the request.
    /// </summary>
    public async Task UpdateGameAsync(GameModel gameModel)
    {
        await _unitOfWork.ExecuteInTransactionAsync(() => UpdateGameInTransactionAsync(gameModel), IsolationLevel.RepeatableRead);
    }

    /// <summary>
    /// Games are soft-deleted: administrators can still see and edit them, and a deleted Northwind game stays hidden.
    /// All translations of the game are removed in the same operation.
    /// </summary>
    public async Task DeleteGameAsync(string key)
    {
        var gameEntity = await catalog.Products.GetByKeyAsync(key)
            ?? throw new NotFoundException($"Game with Key '{key}' was not found.");

        var sqlGame = await _unitOfWork.ProductRepository.GetGameByKeyAsync(key);

        if (sqlGame is null)
        {
            await catalog.Products.MarkNorthwindProductDeletedAsync(gameEntity);
        }
        else
        {
            await localizer.RemoveTranslationsAsync(sqlGame);
            sqlGame.IsDeleted = true;
        }

        await _unitOfWork.SaveAsync();
    }

    public async Task<string> GetGameFilePathAsync(string gameKey)
    {
        if (await catalog.Products.GetByKeyAsync(gameKey) == null)
        {
            throw new NotFoundException($"Game with Key '{gameKey}' was not found.");
        }

        var fileEntity = await _unitOfWork.ProductFileRepository.GetGameFileByKeyAsync(gameKey)
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
        var gameEntities = await catalog.Products.GetAllAsync(CanSeeDeletedGames);
        var games = _mapper.Map<List<GameModel>>(gameEntities);

        await localizer.LocalizeGamesAsync(games);

        return games;
    }

    public async Task<IList<GenreModel>> GetGenresByGameKeyAsync(string key)
    {
        var game = await catalog.Products.GetByKeyAsync(key, CanSeeDeletedGames);

        var genreEntities = game?.ProductCategories
            .Where(pc => pc.Category is not null)
            .Select(pc => pc.Category)
            .ToList() ?? [];

        var genres = _mapper.Map<List<GenreModel>>(genreEntities);
        await localizer.LocalizeGenresAsync(genres);

        return genres;
    }

    public async Task<IList<PlatformModel>> GetPlatformsByGameKeyAsync(string key)
    {
        var platformEntities = await _unitOfWork.ProductRepository.GetPlatformsByGameKeyAsync(key);

        var platforms = _mapper.Map<List<PlatformModel>>(platformEntities);
        await localizer.LocalizePlatformsAsync(platforms);

        return platforms;
    }

    public async Task<PublisherModel> GetPublisherByGameKeyAsync(string key)
    {
        var game = await catalog.Products.GetByKeyAsync(key, CanSeeDeletedGames);

        var publisherEntity = (game is null ? null : game.Supplier ?? await catalog.Suppliers.GetByIdAsync(game.SupplierID))
            ?? throw new NotFoundException($"Game with Key '{key}' was not found or has no publisher.");

        var publisher = _mapper.Map<PublisherModel>(publisherEntity);
        await localizer.LocalizePublishersAsync([publisher]);

        return publisher;
    }

    public async Task<int> GetTotalGamesCountAsync()
    {
        var games = await catalog.Products.GetAllAsync();

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
        var sqlGames = _pipeline.Filter(_unitOfWork.ProductRepository.GetQueryable(CanSeeDeletedGames).AsNoTracking(), filter);
        var sqlCount = await sqlGames.CountAsync();

        // Only the games that can reach the requested page are read from SQL; the page is cut after the merge.
        var sqlCandidates = await _pipeline.Sort(sqlGames, filter).Take(GetCandidateCount(filter)).ToListAsync();
        var northwindGames = await catalog.Products.GetNorthwindOnlyAsync(filter);

        var merged = _pipeline.Sort(sqlCandidates.Concat(northwindGames).AsQueryable(), filter);
        var games = _pipeline.Paginate(merged, filter).ToList();
        var totalCount = sqlCount + northwindGames.Count;

        var gameModels = _mapper.Map<List<GameModel>>(games);
        await localizer.LocalizeGamesAsync(gameModels);

        return new FilteredGamesResponseModel
        {
            Games = gameModels,
            CurrentPage = filter.Page,
            TotalPages = filter.PageSize == (int)PaginationOptions.All
                ? 1
                : (int)Math.Ceiling(totalCount / (double)filter.PageSize),
        };
    }

    private async Task UpdateGameInTransactionAsync(GameModel gameModel)
    {
        var currentGame = await catalog.Products.GetByIdAsync(gameModel.Id, includeDeleted: true)
            ?? throw new NotFoundException($"Game with Id `{gameModel.Id}` was not found.");

        if (currentGame.IsDeleted && !currentUser.HasPermission(Permissions.UpdateDeletedGame))
        {
            throw new ForbiddenException("Deleted games can be edited only by an administrator.");
        }

        await ValidatePlatformsAsync(gameModel.Platforms);

        var existingGame = await catalog.Products.EnsureInGameStoreAsync(currentGame.Key, includeDeleted: true);

        // The key maps a game to its Northwind product, so it cannot be changed for Northwind games.
        gameModel.Key = await catalog.Products.HasNorthwindCounterpartAsync(existingGame)
            || string.Equals(gameModel.Key, existingGame.Key, StringComparison.Ordinal)
            ? existingGame.Key
            : await GenerateUniqueGameKeyAsync(gameModel.Key, gameModel.Name);

        await EnsureGenresInGameStoreAsync(gameModel.Genres);
        var publisher = await EnsurePublisherInGameStoreAsync(gameModel.Publisher);

        UpdateGenres(existingGame, gameModel.Genres);
        UpdatePlatforms(existingGame, gameModel.Platforms);

        if (localizer.IsDefaultLanguage)
        {
            existingGame.ProductName = gameModel.Name;
            existingGame.Description = gameModel.Description;
        }
        else
        {
            await localizer.SaveGameTranslationAsync(existingGame, gameModel.Name, gameModel.Description);
        }

        existingGame.Key = gameModel.Key;
        existingGame.UnitPrice = gameModel.Price;
        existingGame.UnitsInStock = gameModel.UnitInStock;
        existingGame.Discount = gameModel.Discount;
        existingGame.SupplierID = publisher.Id;

        await _unitOfWork.SaveAsync();
        await catalog.Products.SyncUnitsInStockAsync([existingGame]);
    }

    private async Task<GameModel> LocalizeAsync(GameModel game)
    {
        await localizer.LocalizeGamesAsync([game]);

        return game;
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

        while (await catalog.Products.KeyExistsAsync(gameKey))
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
            _ = await catalog.Categories.GetByIdAsync(genreId)
                ?? throw new NotFoundException("Genre", genreId);

            await catalog.Categories.EnsureInGameStoreAsync(genreId);
        }
    }

    private async Task<SupplierEntity> EnsurePublisherInGameStoreAsync(Guid publisherId)
    {
        _ = await catalog.Suppliers.GetByIdAsync(publisherId)
            ?? throw new NotFoundException($"Publisher with ID '{publisherId}' was not found.");

        return await catalog.Suppliers.EnsureInGameStoreAsync(publisherId);
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
