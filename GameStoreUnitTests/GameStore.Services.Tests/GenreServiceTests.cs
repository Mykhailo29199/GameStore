using System.Linq.Expressions;
using AutoMapper;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;
using Moq;
using Xunit;
using static GameStoreUnitTests.GameStore.Services.Tests.TestConstants;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class GenreServiceTests
{
    private readonly GenreService _genreService;
    private readonly Mock<ISqlServerUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICategoryCatalog> _categoryCatalogMock;
    private readonly Mock<IProductCatalog> _productCatalogMock;

    public GenreServiceTests()
    {
        _unitOfWorkMock = new Mock<ISqlServerUnitOfWork> { DefaultValue = DefaultValue.Mock };
        _mapperMock = new Mock<IMapper>();
        _categoryCatalogMock = new Mock<ICategoryCatalog>();
        _productCatalogMock = new Mock<IProductCatalog>();

        _categoryCatalogMock.Setup(x => x.GetAllAsync()).ReturnsAsync(ExistingGenres);
        _mapperMock.Setup(m => m.Map<List<GenreModel>>(It.IsAny<List<CategoryEntity>>()))
            .Returns((object genres) => ((List<CategoryEntity>)genres)
                .Select(g => new GenreModel { Id = g.Id, Name = g.Name, ParentGenreId = g.ParentCategoryId })
                .ToList());

        _genreService = new GenreService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _categoryCatalogMock.Object,
            _productCatalogMock.Object,
            TestLocalization.CreateLocalizer(_unitOfWorkMock));
    }

    #region CreateGenreAsync
    [Fact]
    public async Task CreateGenreAsync_WhenGenreIsItsOwnParent_ShouldThrowABadRequestException()
    {
        // Arrange
        var genre = ExistingGenres.First();
        GenreModel genreModel = CreateGenreModel(id: genre.Id, name: "New genre", parentGenreId: genre.Id);

        _categoryCatalogMock.Setup(x => x.GetByIdAsync(genre.Id)).ReturnsAsync(genre);

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _genreService.CreateGenreAsync(genreModel));

        // Assert
        Assert.Equal("A genre cannot be its own parent.", exception.Message);
    }

    [Fact]
    public async Task CreateGenreAsync_WhenParentGenreDoesNotExist_ShouldThrowANotFoundException()
    {
        // Arrange
        GenreModel genreModel = CreateGenreModel(
            id: NonExistingGenreId,
            name: "New genre",
            parentGenreId: NonExistingGenreId);

        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(NonExistingGenreId))
            .ReturnsAsync((CategoryEntity?)null);

        // Act
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.CreateGenreAsync(genreModel));

        // Assert
        Assert.Equal(
            $"Parent genre with Id {NonExistingGenreId} does not exist.",
            exception.Message);
    }

    [Fact]
    public async Task CreateGenreAsync_WhenNameExistsInAnyDatabase_ShouldThrowBadRequest()
    {
        // Arrange
        GenreModel genreModel = CreateGenreModel(
            id: NonExistingGenreId,
            name: ExistingGenre1.Name.ToUpperInvariant());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(
            () => _genreService.CreateGenreAsync(genreModel));
    }

    [Fact]
    public async Task CreateGenreAsync_ValidData_AddsGenreAndSaves()
    {
        // Arrange
        GenreModel genreModel = CreateGenreModel(
            id: NonExistingGenreId,
            name: "New genre",
            parentGenreId: ExistingGenreId);

        CategoryEntity genreEntity = new()
        {
            Id = genreModel.Id,
            Name = genreModel.Name,
        };

        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(ExistingGenreId))
            .ReturnsAsync(ExistingGenre1);

        _mapperMock
            .Setup(m => m.Map<CategoryEntity>(genreModel))
            .Returns(genreEntity);

        // Act
        await _genreService.CreateGenreAsync(genreModel);

        // Assert
        _categoryCatalogMock.Verify(
            x => x.EnsureInGameStoreAsync(ExistingGenreId),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.CategoryRepository.AddAsync(genreEntity),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveAsync(),
            Times.Once);
    }
    #endregion

    #region GetGenreByIdAsync

    [Fact]
    public async Task GetGenreByIdAsync_WhenGenreDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(NonExistingGenreId))
            .ReturnsAsync((CategoryEntity?)null);

        // Act
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.GetGenreByIdAsync(NonExistingGenreId));

        // Assert
        Assert.Equal(
            $"Genre with Id {NonExistingGenreId} does not exist.",
            exception.Message);
    }

    [Fact]
    public async Task GetGenreByIdAsync_ValidData_ReturnsMappedGenre()
    {
        // Arrange
        GenreModel genreModel = CreateGenreModel(
            id: ExistingGenre1.Id,
            name: ExistingGenre1.Name);

        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(ExistingGenre1.Id))
            .ReturnsAsync(ExistingGenre1);

        _mapperMock
            .Setup(m => m.Map<GenreModel>(ExistingGenre1))
            .Returns(genreModel);

        // Act
        GenreModel result = await _genreService.GetGenreByIdAsync(ExistingGenre1.Id);

        // Assert
        Assert.Equal(genreModel, result);
    }

    [Fact]
    public async Task GetAllGenresAsync_ShouldReturnGenresFromBothDatabases()
    {
        // Arrange
        List<GenreModel> mappedGenres =
        [
            new GenreModel { Id = ExistingGenre1.Id, Name = ExistingGenre1.Name },
            new GenreModel { Id = ExistingGenre2.Id, Name = ExistingGenre2.Name },
        ];

        _mapperMock
            .Setup(m => m.Map<List<GenreModel>>(ExistingGenres))
            .Returns(mappedGenres);

        // Act
        var result = await _genreService.GetAllGenresAsync();

        // Assert
        Assert.Equal(mappedGenres, result);
    }

    #endregion

    #region GetGenresByParentGenreIdAsync
    [Fact]
    public async Task GetGenresByParentGenreIdAsync_WhenParentGenreDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(NonExistingGenreId))
            .ReturnsAsync((CategoryEntity?)null);

        // Act
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.GetGenresByParentGenreIdAsync(NonExistingGenreId));

        // Assert
        Assert.Equal(
            $"Parent genre with Id {NonExistingGenreId} does not exist.",
            exception.Message);
    }

    [Fact]
    public async Task GetGenresByParentGenreIdAsync_ValidData_ReturnsMappedGenres()
    {
        // Arrange
        Guid parentGenreId = ExistingGenreId;

        List<CategoryEntity> genreEntities =
        [
            new CategoryEntity
            {
                Id = AnotherExistingGenreId,
                Name = ExistingGenre2.Name,
                ParentCategoryId = parentGenreId,
            },
        ];

        List<GenreModel> mappedGenres =
        [
            new GenreModel
            {
                Id = genreEntities[0].Id,
                Name = genreEntities[0].Name,
                ParentGenreId = parentGenreId,
            },
        ];

        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(parentGenreId))
            .ReturnsAsync(ExistingGenre1);

        _unitOfWorkMock
            .Setup(x => x.CategoryRepository.FindAsync(
                It.IsAny<Expression<Func<CategoryEntity, bool>>>()))
            .ReturnsAsync(genreEntities);

        _mapperMock
            .Setup(x => x.Map<List<GenreModel>>(genreEntities))
            .Returns(mappedGenres);

        // Act
        IList<GenreModel> result =
            await _genreService.GetGenresByParentGenreIdAsync(parentGenreId);

        // Assert
        Assert.Equal(mappedGenres, result);
    }

    #endregion

    #region UpdateGenreAsync

    [Fact]
    public async Task UpdateGenreAsync_WhenGenreDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        GenreModel genreModel = CreateGenreModel(
            id: NonExistingGenreId);

        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(genreModel.Id))
            .ReturnsAsync((CategoryEntity?)null);

        // Act
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.UpdateGenreAsync(genreModel));

        // Assert
        Assert.Equal(
            $"Genre with Id {genreModel.Id} does not exist.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateGenreAsync_WhenParentGenreDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        GenreModel genreModel = CreateGenreModel(
            id: ExistingGenreId,
            parentGenreId: NonExistingGenreId);

        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(genreModel.Id))
            .ReturnsAsync(ExistingGenre1);

        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(genreModel.ParentGenreId!.Value))
            .ReturnsAsync((CategoryEntity?)null);

        // Act
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.UpdateGenreAsync(genreModel));

        // Assert
        Assert.Equal(
            $"Parent genre with Id {genreModel.ParentGenreId} does not exist.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateGenreAsync_ValidData_ShouldUpdateGameStoreGenreAndSave()
    {
        // Arrange
        GenreModel genreModel = CreateGenreModel(
            id: ExistingGenreId,
            name: "Renamed",
            parentGenreId: AnotherExistingGenreId);

        CategoryEntity gameStoreGenre = new() { Id = ExistingGenreId, Name = ExistingGenre1.Name };

        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(genreModel.Id))
            .ReturnsAsync(ExistingGenre1);

        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(AnotherExistingGenreId))
            .ReturnsAsync(ExistingGenre2);

        _categoryCatalogMock
            .Setup(x => x.EnsureInGameStoreAsync(ExistingGenreId))
            .ReturnsAsync(gameStoreGenre);

        // Act
        await _genreService.UpdateGenreAsync(genreModel);

        // Assert
        Assert.Equal("Renamed", gameStoreGenre.Name);
        Assert.Equal(AnotherExistingGenreId, gameStoreGenre.ParentCategoryId);

        _unitOfWorkMock.Verify(
            x => x.SaveAsync(),
            Times.Once);
    }

    #endregion

    #region DeleteGenreAsync

    [Fact]
    public async Task DeleteGenreAsync_WhenGenreDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(NonExistingGenreId))
            .ReturnsAsync((CategoryEntity?)null);

        // Act
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.DeleteGenreAsync(NonExistingGenreId));

        // Assert
        Assert.Equal(
            $"Genre with Id {NonExistingGenreId} does not exist.",
            exception.Message);
    }

    [Fact]
    public async Task DeleteGenreAsync_WhenGenreExistsOnlyInGameStore_ShouldDeleteGenreAndSave()
    {
        // Arrange
        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(ExistingGenreId))
            .ReturnsAsync(ExistingGenre1);

        _categoryCatalogMock
            .Setup(x => x.HasNorthwindCounterpartAsync(ExistingGenre1))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.CategoryRepository.GetWithSubGenresAsync(ExistingGenreId))
            .ReturnsAsync(ExistingGenre1);

        // Act
        await _genreService.DeleteGenreAsync(ExistingGenreId);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.CategoryRepository.DeleteAsync(ExistingGenre1),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveAsync(),
            Times.Once);
    }

    [Fact]
    public async Task DeleteGenreAsync_WhenGenreExistsInNorthwind_ShouldSoftDeleteGameStoreCopy()
    {
        // Arrange
        CategoryEntity gameStoreCopy = new() { Id = ExistingGenreId, Name = ExistingGenre1.Name };

        _categoryCatalogMock
            .Setup(x => x.GetByIdAsync(ExistingGenreId))
            .ReturnsAsync(ExistingGenre1);

        _categoryCatalogMock
            .Setup(x => x.HasNorthwindCounterpartAsync(ExistingGenre1))
            .ReturnsAsync(true);

        _categoryCatalogMock
            .Setup(x => x.EnsureInGameStoreAsync(ExistingGenreId))
            .ReturnsAsync(gameStoreCopy);

        // Act
        await _genreService.DeleteGenreAsync(ExistingGenreId);

        // Assert
        Assert.True(gameStoreCopy.IsDeleted);

        _unitOfWorkMock.Verify(
            x => x.CategoryRepository.DeleteAsync(It.IsAny<CategoryEntity>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveAsync(),
            Times.Once);
    }

    #endregion

    #region GetGamesByGenreIdAsync

    [Fact]
    public async Task GetGamesByGenreIdAsync_WhenGenreDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _categoryCatalogMock.Setup(x => x.GetByIdAsync(NonExistingGenreId))
            .ReturnsAsync((CategoryEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.GetGamesByGenreIdAsync(NonExistingGenreId));

        Assert.Equal($"Genre with id '{NonExistingGenreId}' does not exist.", exception.Message);
    }

    [Fact]
    public async Task GetGamesByGenreIdAsync_WhenGenreExists_ReturnsGamesOfGenreFromBothDatabases()
    {
        // Arrange
        _categoryCatalogMock.Setup(x => x.GetByIdAsync(ExistingGenreId))
            .ReturnsAsync(ExistingGenre1);

        var gameStoreGame = new ProductEntity
        {
            Id = ExistingGameId,
            ProductName = "Game 1",
            Key = ExistingGameKey,
            ProductCategories = [new ProductCategoryEntity { CategoryId = ExistingGenreId }],
        };

        var northwindGame = new ProductEntity
        {
            Id = ExistingGameId2,
            ProductName = "Chai",
            Key = "nw_chai",
            ProductCategories = [new ProductCategoryEntity { CategoryId = ExistingGenreId }],
        };

        var otherGenreGame = new ProductEntity
        {
            Id = NonExistingGameId,
            ProductName = "Other",
            Key = ExistingGameKey2,
            ProductCategories = [new ProductCategoryEntity { CategoryId = AnotherExistingGenreId }],
        };

        _productCatalogMock.Setup(x => x.GetAllAsync(It.IsAny<bool>()))
            .ReturnsAsync([gameStoreGame, northwindGame, otherGenreGame]);

        List<GameModel> gameModels =
        [
            new GameModel { Id = gameStoreGame.Id, Name = gameStoreGame.ProductName },
            new GameModel { Id = northwindGame.Id, Name = northwindGame.ProductName },
        ];

        _mapperMock.Setup(m => m.Map<List<GameModel>>(It.Is<List<ProductEntity>>(l =>
                l.Count == 2 && l.Contains(gameStoreGame) && l.Contains(northwindGame))))
            .Returns(gameModels);

        // Act
        var result = await _genreService.GetGamesByGenreIdAsync(ExistingGenreId);

        // Assert
        Assert.Equal(gameModels, result);
    }

    #endregion

    private static GenreModel CreateGenreModel(
    Guid? id = null,
    string name = "",
    Guid? parentGenreId = null)
    {
        return new GenreModel
        {
            Id = id ?? Guid.Empty,
            Name = name,
            ParentGenreId = parentGenreId,
        };
    }
}
