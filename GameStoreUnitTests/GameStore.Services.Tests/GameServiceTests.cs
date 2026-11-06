using AutoMapper;
using BusinessLogicLayer.Catalog;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines;
using BusinessLogicLayer.Pipelines.Steps;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Enums.FilterOptionsEnums;
using GameStore.Shared.Exceptions;
using GameStore.Shared.FileSystem;
using Moq;
using Xunit;
using static GameStoreUnitTests.GameStore.Services.Tests.TestConstants;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class GameServiceTests
{
    private const string NorthwindGameKey = "nw_chai";

    private readonly GameService _gameService;
    private readonly Mock<ISqlServerUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly Mock<IProductCatalog> _productCatalogMock;
    private readonly Mock<ICategoryCatalog> _categoryCatalogMock;
    private readonly Mock<ISupplierCatalog> _supplierCatalogMock;

    public GameServiceTests()
    {
        _unitOfWorkMock = new Mock<ISqlServerUnitOfWork> { DefaultValue = DefaultValue.Mock };
        _mapperMock = new Mock<IMapper>();
        _fileSystemMock = new Mock<IFileSystem>();
        _productCatalogMock = new Mock<IProductCatalog>();
        _categoryCatalogMock = new Mock<ICategoryCatalog>();
        _supplierCatalogMock = new Mock<ISupplierCatalog>();

        _unitOfWorkMock.Setup(x => x.PlatformRepository.GetAllAsync()).ReturnsAsync(ExistingPlatforms);
        _unitOfWorkMock.Setup(x => x.ProductCategoryRepository.GetByGameIdAsync(It.IsAny<Guid>())).ReturnsAsync([]);
        _unitOfWorkMock.Setup(x => x.ProductPlatformRepository.GetByGameIdAsync(It.IsAny<Guid>())).ReturnsAsync([]);

        _gameService = new GameService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _fileSystemMock.Object,
            new GameFilterPipeline([], new SortingStep(), new PaginationStep()),
            new StoreCatalog(_productCatalogMock.Object, _categoryCatalogMock.Object, _supplierCatalogMock.Object));
    }

    [Fact]
    public async Task CreateGameAsync_WhenPublisherDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var gameModel = CreateGameModel(name: "New Game", publisher: NonExistingPublisherId);

        _supplierCatalogMock.Setup(x => x.GetByIdAsync(NonExistingPublisherId))
            .ReturnsAsync((SupplierEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.CreateGameAsync(gameModel));

        // Assert
        Assert.Equal($"Publisher with ID '{NonExistingPublisherId}' was not found.", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WhenPlatformDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var gameModel = CreateGameModel(name: "New Game", platforms: [NonExistingPlatformId]);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.CreateGameAsync(gameModel));

        // Assert
        Assert.Equal($"Platform with ID '{NonExistingPlatformId}' was not found.", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WhenGenreDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var gameModel = CreateGameModel(name: "New Game", platforms: ExistingPlatformIds, genres: [NonExistingGenreId]);

        _categoryCatalogMock.Setup(x => x.GetByIdAsync(NonExistingGenreId))
            .ReturnsAsync((CategoryEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.CreateGameAsync(gameModel));

        // Assert
        Assert.Equal($"Genre with ID '{NonExistingGenreId}' was not found.", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WhenDataIsValid_ShouldCopyReferencesAndCreateGame()
    {
        // Arrange
        var gameModel = CreateGameModel(
            name: "New Game",
            publisher: ExistingPublisherId,
            platforms: ExistingPlatformIds,
            genres: ExistingGenreIds);

        var publisher = new SupplierEntity { Id = ExistingPublisherId, CompanyName = "Publisher" };
        SetupExistingReferences(publisher);

        var gameEntity = new ProductEntity();
        _mapperMock.Setup(x => x.Map<ProductEntity>(gameModel)).Returns(gameEntity);

        // Act
        await _gameService.CreateGameAsync(gameModel);

        // Assert
        Assert.Equal(ExistingPublisherId, gameEntity.SupplierID);
        _categoryCatalogMock.Verify(x => x.EnsureInGameStoreAsync(ExistingGenreId), Times.Once);
        _categoryCatalogMock.Verify(x => x.EnsureInGameStoreAsync(AnotherExistingGenreId), Times.Once);
        _supplierCatalogMock.Verify(x => x.EnsureInGameStoreAsync(ExistingPublisherId), Times.Once);
        _unitOfWorkMock.Verify(x => x.ProductRepository.AddAsync(gameEntity), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateGameAsync_WhenKeyExistsInAnyDatabase_ShouldGenerateUniqueKey()
    {
        // Arrange
        var gameModel = CreateGameModel(name: "My Game", publisher: ExistingPublisherId);
        SetupExistingReferences(new SupplierEntity { Id = ExistingPublisherId, CompanyName = "Publisher" });

        _productCatalogMock.Setup(x => x.KeyExistsAsync("my-game")).ReturnsAsync(true);
        _productCatalogMock.Setup(x => x.KeyExistsAsync("my-game-1")).ReturnsAsync(false);
        _mapperMock.Setup(x => x.Map<ProductEntity>(gameModel)).Returns(new ProductEntity());

        // Act
        await _gameService.CreateGameAsync(gameModel);

        // Assert
        Assert.Equal("my-game-1", gameModel.Key);
    }

    [Fact]
    public async Task GetGameByKeyAsync_WhenGameDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _productCatalogMock.Setup(x => x.GetByKeyAsync(NonExistingGameKey))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.GetGameByKeyAsync(NonExistingGameKey));

        // Assert
        Assert.Equal($"Game with Key '{NonExistingGameKey}' was not found.", exception.Message);
    }

    [Fact]
    public async Task GetGameByKeyAsync_WhenGameExists_ShouldIncreaseViewsAndReturnGame()
    {
        // Arrange
        var gameEntity = new ProductEntity { Key = NorthwindGameKey };
        var gameModel = new GameModel { Key = NorthwindGameKey };

        _productCatalogMock.Setup(x => x.GetByKeyAsync(NorthwindGameKey)).ReturnsAsync(gameEntity);
        _mapperMock.Setup(x => x.Map<GameModel>(gameEntity)).Returns(gameModel);

        // Act
        var result = await _gameService.GetGameByKeyAsync(NorthwindGameKey);

        // Assert
        Assert.Equal(gameModel, result);
        _productCatalogMock.Verify(x => x.IncreaseViewsAsync(NorthwindGameKey), Times.Once);
    }

    [Fact]
    public async Task UpdateGameAsync_WhenGameDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var gameModel = CreateGameModel();
        gameModel.Id = NonExistingGameId;

        _productCatalogMock.Setup(x => x.GetByIdAsync(NonExistingGameId))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.UpdateGameAsync(gameModel));

        // Assert
        Assert.Equal($"Game with Id `{NonExistingGameId}` was not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateGameAsync_WhenGameComesFromNorthwind_ShouldUpdateGameStoreCopyAndKeepKey()
    {
        // Arrange
        var gameModel = CreateGameModel(
            key: "changed-key",
            name: "Updated Name",
            publisher: ExistingPublisherId,
            platforms: ExistingPlatformIds,
            genres: ExistingGenreIds);
        gameModel.Id = ExistingGameId;
        gameModel.Price = 42;

        var northwindGame = new ProductEntity { Id = ExistingGameId, Key = NorthwindGameKey };

        // The copy is not saved yet and keeps the genre it had in Northwind.
        var gameStoreCopy = new ProductEntity { Id = ExistingGameId, Key = NorthwindGameKey, ProductName = "Chai" };
        gameStoreCopy.ProductCategories.Add(new ProductCategoryEntity { ProductId = ExistingGameId, CategoryId = Guid.NewGuid() });

        _productCatalogMock.Setup(x => x.GetByIdAsync(ExistingGameId)).ReturnsAsync(northwindGame);
        _productCatalogMock.Setup(x => x.EnsureInGameStoreAsync(NorthwindGameKey)).ReturnsAsync(gameStoreCopy);
        _productCatalogMock.Setup(x => x.HasNorthwindCounterpartAsync(gameStoreCopy)).ReturnsAsync(true);
        SetupExistingReferences(new SupplierEntity { Id = ExistingPublisherId, CompanyName = "Publisher" });

        // Act
        await _gameService.UpdateGameAsync(gameModel);

        // Assert
        Assert.Equal(NorthwindGameKey, gameStoreCopy.Key);
        Assert.Equal("Updated Name", gameStoreCopy.ProductName);
        Assert.Equal(42, gameStoreCopy.UnitPrice);
        Assert.Equal(ExistingPublisherId, gameStoreCopy.SupplierID);
        Assert.Equal(ExistingGenreIds.Order(), gameStoreCopy.ProductCategories.Select(pc => pc.CategoryId).Order());
        Assert.Equal(ExistingPlatformIds.Order(), gameStoreCopy.ProductPlatforms.Select(pp => pp.PlatformId).Order());
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _productCatalogMock.Setup(x => x.GetByKeyAsync(NonExistingGameKey))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.DeleteGameAsync(NonExistingGameKey));

        // Assert
        Assert.Equal($"Game with Key '{NonExistingGameKey}' was not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameExistsOnlyInNorthwind_ShouldMarkItDeletedInGameStore()
    {
        // Arrange
        var northwindGame = new ProductEntity { Key = NorthwindGameKey };

        _productCatalogMock.Setup(x => x.GetByKeyAsync(NorthwindGameKey)).ReturnsAsync(northwindGame);
        _unitOfWorkMock.Setup(x => x.ProductRepository.GetGameByKeyAsync(NorthwindGameKey))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        await _gameService.DeleteGameAsync(NorthwindGameKey);

        // Assert
        _productCatalogMock.Verify(x => x.MarkNorthwindProductDeletedAsync(northwindGame), Times.Once);
        _unitOfWorkMock.Verify(x => x.ProductRepository.DeleteAsync(It.IsAny<ProductEntity>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameStoreCopyOfNorthwindGame_ShouldSoftDelete()
    {
        // Arrange
        var gameStoreGame = new ProductEntity { Key = NorthwindGameKey };

        _productCatalogMock.Setup(x => x.GetByKeyAsync(NorthwindGameKey)).ReturnsAsync(gameStoreGame);
        _productCatalogMock.Setup(x => x.HasNorthwindCounterpartAsync(gameStoreGame)).ReturnsAsync(true);
        _unitOfWorkMock.Setup(x => x.ProductRepository.GetGameByKeyAsync(NorthwindGameKey)).ReturnsAsync(gameStoreGame);

        // Act
        await _gameService.DeleteGameAsync(NorthwindGameKey);

        // Assert
        Assert.True(gameStoreGame.IsDeleted);
        _unitOfWorkMock.Verify(x => x.ProductRepository.DeleteAsync(It.IsAny<ProductEntity>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameExistsOnlyInGameStore_ShouldDeleteGame()
    {
        // Arrange
        var existingGame = new ProductEntity { Key = ExistingGameKey };

        _productCatalogMock.Setup(x => x.GetByKeyAsync(ExistingGameKey)).ReturnsAsync(existingGame);
        _unitOfWorkMock.Setup(x => x.ProductRepository.GetGameByKeyAsync(ExistingGameKey)).ReturnsAsync(existingGame);

        // Act
        await _gameService.DeleteGameAsync(ExistingGameKey);

        // Assert
        _unitOfWorkMock.Verify(x => x.ProductRepository.DeleteAsync(existingGame), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGameFilePathAsync_WhenGameFileEntityDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _productCatalogMock.Setup(x => x.GetByKeyAsync(ExistingGameKey)).ReturnsAsync(new ProductEntity());
        _unitOfWorkMock.Setup(x => x.ProductFileRepository.GetGameFileByKeyAsync(ExistingGameKey))
            .ReturnsAsync((ProductFileEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.GetGameFilePathAsync(ExistingGameKey));

        // Assert
        Assert.Equal($"Game file for game with Key '{ExistingGameKey}' was not found.", exception.Message);
    }

    [Fact]
    public async Task GetGameFilePathAsync_WhenFileExists_ShouldReturnFilePath()
    {
        // Arrange
        string filePath = "tempPath";

        _productCatalogMock.Setup(x => x.GetByKeyAsync(ExistingGameKey)).ReturnsAsync(new ProductEntity());
        _unitOfWorkMock.Setup(x => x.ProductFileRepository.GetGameFileByKeyAsync(ExistingGameKey))
            .ReturnsAsync(new ProductFileEntity { FilePath = filePath });
        _fileSystemMock.Setup(x => x.FileExists(filePath)).Returns(true);

        // Act
        string result = await _gameService.GetGameFilePathAsync(ExistingGameKey);

        // Assert
        Assert.Equal(filePath, result);
    }

    [Fact]
    public async Task GetFilteredGamesAsync_ShouldReturnGamesFromBothDatabases()
    {
        // Arrange
        var service = CreateServiceWithGames(
            [CreateGame("Game Store Game", ExistingGameKey, views: 2)],
            [CreateGame("Chai", NorthwindGameKey, views: 1)]);

        // Act
        var result = await service.GetFilteredGamesAsync(new GameFilterModel
        {
            Page = 1,
            PageSize = (int)PaginationOptions.All,
        });

        // Assert
        Assert.Equal([ExistingGameKey, NorthwindGameKey], result.Games.Select(g => g.Key));
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetFilteredGamesAsync_ShouldCountPagesAfterFiltering()
    {
        // Arrange
        var service = CreateServiceWithGames(
            [CreateGame("Racing", "racing", views: 3), CreateGame("Chess", "chess", views: 2), CreateGame("Golf", "golf", views: 1)],
            [CreateGame("Racing Tea", NorthwindGameKey, views: 0)]);

        // Act
        var result = await service.GetFilteredGamesAsync(new GameFilterModel
        {
            Name = "Racing",
            Page = 1,
            PageSize = 1,
        });

        // Assert
        Assert.Equal(["racing"], result.Games.Select(g => g.Key));
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetFilteredGamesAsync_ShouldSortAndPageGamesOfBothDatabasesTogether()
    {
        // Arrange
        var service = CreateServiceWithGames(
            [CreateGame("First", "first", views: 10), CreateGame("Third", "third", views: 5)],
            [CreateGame("Second", NorthwindGameKey, views: 7)]);

        // Act
        var result = await service.GetFilteredGamesAsync(new GameFilterModel
        {
            Sort = SortingOptions.MostPopular,
            Page = 2,
            PageSize = 1,
        });

        // Assert
        Assert.Equal([NorthwindGameKey], result.Games.Select(g => g.Key));
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetGenresByGameKeyAsync_ShouldReturnGenresOfCatalogGame()
    {
        // Arrange
        var game = new ProductEntity
        {
            Key = NorthwindGameKey,
            ProductCategories =
            [
                new ProductCategoryEntity { CategoryId = ExistingGenreId, Category = ExistingGenre1 },
            ],
        };

        List<GenreModel> mappedGenres = [new GenreModel { Id = ExistingGenreId, Name = ExistingGenre1.Name }];

        _productCatalogMock.Setup(x => x.GetByKeyAsync(NorthwindGameKey)).ReturnsAsync(game);
        _mapperMock.Setup(x => x.Map<IList<GenreModel>>(It.Is<List<CategoryEntity>>(l => l.Single() == ExistingGenre1)))
            .Returns(mappedGenres);

        // Act
        var result = await _gameService.GetGenresByGameKeyAsync(NorthwindGameKey);

        // Assert
        Assert.Equal(mappedGenres, result);
    }

    [Fact]
    public async Task GetPublisherByGameKeyAsync_WhenPublisherIsNotLoaded_ShouldResolveItFromCatalog()
    {
        // Arrange
        var publisher = new SupplierEntity { Id = ExistingPublisherId, CompanyName = "Exotic Liquids" };
        var game = new ProductEntity { Key = NorthwindGameKey, SupplierID = ExistingPublisherId, Supplier = null };
        var publisherModel = new PublisherModel { Id = ExistingPublisherId, CompanyName = publisher.CompanyName };

        _productCatalogMock.Setup(x => x.GetByKeyAsync(NorthwindGameKey)).ReturnsAsync(game);
        _supplierCatalogMock.Setup(x => x.GetByIdAsync(ExistingPublisherId)).ReturnsAsync(publisher);
        _mapperMock.Setup(x => x.Map<PublisherModel>(publisher)).Returns(publisherModel);

        // Act
        var result = await _gameService.GetPublisherByGameKeyAsync(NorthwindGameKey);

        // Assert
        Assert.Equal(publisherModel, result);
    }

    private static GameModel CreateGameModel(
        string key = "",
        string name = "",
        Guid publisher = default,
        IEnumerable<Guid>? platforms = null,
        IEnumerable<Guid>? genres = null)
    {
        return new GameModel
        {
            Key = key,
            Name = name,
            Publisher = publisher,
            Platforms = platforms?.ToList() ?? [],
            Genres = genres?.ToList() ?? [],
        };
    }

    private static ProductEntity CreateGame(string name, string key, int views)
    {
        return new ProductEntity { Id = Guid.NewGuid(), ProductName = name, Key = key, Views = views };
    }

    /// <summary>
    /// Creates a service with real filter, sorting and paging steps over the given Game Store and Northwind games.
    /// </summary>
    private GameService CreateServiceWithGames(List<ProductEntity> gameStoreGames, List<ProductEntity> northwindGames)
    {
        _unitOfWorkMock.Setup(x => x.ProductRepository.GetQueryable()).Returns(new AsyncQueryable<ProductEntity>(gameStoreGames));
        _productCatalogMock.Setup(x => x.GetNorthwindOnlyAsync(It.IsAny<GameFilterModel>())).ReturnsAsync(northwindGames);
        _mapperMock.Setup(x => x.Map<List<GameModel>>(It.IsAny<List<ProductEntity>>()))
            .Returns((object games) => ((List<ProductEntity>)games).Select(g => new GameModel { Key = g.Key }).ToList());

        return new GameService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _fileSystemMock.Object,
            new GameFilterPipeline([new NameFilterStep()], new SortingStep(), new PaginationStep()),
            new StoreCatalog(_productCatalogMock.Object, _categoryCatalogMock.Object, _supplierCatalogMock.Object));
    }

    private void SetupExistingReferences(SupplierEntity publisher)
    {
        _categoryCatalogMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => ExistingGenres.FirstOrDefault(g => g.Id == id));
        _categoryCatalogMock.Setup(x => x.EnsureInGameStoreAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => ExistingGenres.First(g => g.Id == id));

        _supplierCatalogMock.Setup(x => x.GetByIdAsync(publisher.Id)).ReturnsAsync(publisher);
        _supplierCatalogMock.Setup(x => x.EnsureInGameStoreAsync(publisher.Id)).ReturnsAsync(publisher);
    }
}
