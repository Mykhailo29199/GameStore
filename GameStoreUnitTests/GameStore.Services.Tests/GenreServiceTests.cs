using System.Linq.Expressions;
using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;
using Moq;
using Xunit;
using static GameStoreUnitTests.GameStore.Services.Tests.TestConstants;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class GenreServiceTests
{
    private readonly GenreService _genreService;
    private readonly Mock<IUnitOfWork<GameStoreContext>> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;

    public GenreServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork<GameStoreContext>>();
        _mapperMock = new Mock<IMapper>();
        _genreService = new GenreService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    #region CreateGenreAsync
    [Fact]
    public async Task CreateGenreAsync_WhenParentGenreDoesNotExist_ShouldThrowANotFoundException()
    {
        // Arrange
        GenreModel genreModel = CreateGenreModel(
            id: NonExistingGenreId,
            name: ExistingGenre1.Name,
            parentGenreId: NonExistingGenreId);

        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetByIdAsync(NonExistingGenreId))
            .ReturnsAsync((GenreEntity?)null);

        // Act
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.CreateGenreAsync(genreModel));

        // Assert
        Assert.Equal(
            $"Parent genre with Id {NonExistingGenreId} does not exist.",
            exception.Message);
    }

    [Fact]
    public async Task CreateGenreAsync_ValidData_AddsGenreAndSaves()
    {
        // Arrange
        GenreModel genreModel = CreateGenreModel(
            id: ExistingGenreId,
            name: ExistingGenre1.Name);

        GenreEntity genreEntity = new()
        {
            Id = genreModel.Id,
            Name = genreModel.Name,
        };

        _mapperMock
            .Setup(m => m.Map<GenreEntity>(genreModel))
            .Returns(genreEntity);

        _unitOfWorkMock
            .Setup(u => u.GenreRepository.AddAsync(genreEntity))
            .Returns(Task.CompletedTask);

        // Act
        await _genreService.CreateGenreAsync(genreModel);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.GenreRepository.AddAsync(genreEntity),
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
        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetByIdAsync(NonExistingGenreId))
            .ReturnsAsync((GenreEntity?)null);

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

        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetByIdAsync(ExistingGenre1.Id))
            .ReturnsAsync(ExistingGenre1);

        _mapperMock
            .Setup(m => m.Map<GenreModel>(ExistingGenre1))
            .Returns(genreModel);

        // Act
        GenreModel result = await _genreService.GetGenreByIdAsync(ExistingGenre1.Id);

        // Assert
        Assert.Equal(genreModel, result);
    }

    #endregion

    #region GetGenresByParentGenreIdAsync
    [Fact]
    public async Task GetGenresByParentGenreIdAsync_WhenParentGenreDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetByIdAsync(NonExistingGenreId))
            .ReturnsAsync((GenreEntity?)null);

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
        GenreEntity parentGenre = ExistingGenre1;

        List<GenreEntity> genreEntities =
        [
            new GenreEntity
            {
                Id = AnotherExistingGenreId,
                Name = ExistingGenre2.Name,
                ParentGenreId = parentGenreId,
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

        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetByIdAsync(parentGenreId))
            .ReturnsAsync(parentGenre);

        _unitOfWorkMock
            .Setup(x => x.GenreRepository.FindAsync(
                It.IsAny<Expression<Func<GenreEntity, bool>>>()))
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

        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetByIdAsync(genreModel.Id))
            .ReturnsAsync((GenreEntity?)null);

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

        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetByIdAsync(genreModel.Id))
            .ReturnsAsync(ExistingGenre1);

        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetByIdAsync(genreModel.ParentGenreId!.Value))
            .ReturnsAsync((GenreEntity?)null);

        // Act
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.UpdateGenreAsync(genreModel));

        // Assert
        Assert.Equal(
            $"Parent genre with Id {genreModel.ParentGenreId} does not exist.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateGenreAsync_ValidData_ShouldUpdateGenreAndSave()
    {
        // Arrange
        GenreModel genreModel = CreateGenreModel(
            id: ExistingGenreId,
            parentGenreId: AnotherExistingGenreId);

        GenreEntity existingGenre = ExistingGenre1;

        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetByIdAsync(genreModel.Id))
            .ReturnsAsync(existingGenre);

        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetByIdAsync(genreModel.ParentGenreId!.Value))
            .ReturnsAsync(ExistingGenre2);

        // Act
        await _genreService.UpdateGenreAsync(genreModel);

        // Assert
        _mapperMock.Verify(
            x => x.Map(genreModel, existingGenre),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.GenreRepository.UpdateAsync(existingGenre),
            Times.Once);

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
        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetWithSubGenresAsync(NonExistingGenreId))
            .ReturnsAsync((GenreEntity?)null);

        // Act
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.DeleteGenreAsync(NonExistingGenreId));

        // Assert
        Assert.Equal(
            $"Genre with Id {NonExistingGenreId} does not exist.",
            exception.Message);
    }

    [Fact]
    public async Task DeleteGenreAsync_ValidData_ShouldDeleteGenreAndSave()
    {
        // Arrange
        _unitOfWorkMock
            .Setup(x => x.GenreRepository.GetWithSubGenresAsync(ExistingGenreId))
            .ReturnsAsync(ExistingGenre1);

        // Act
        await _genreService.DeleteGenreAsync(ExistingGenreId);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.GenreRepository.DeleteAsync(ExistingGenre1),
            Times.Once);

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
        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(NonExistingGenreId))
            .ReturnsAsync((GenreEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.GetGamesByGenreIdAsync(NonExistingGenreId));

        Assert.Equal($"Genre with id '{NonExistingGenreId}' does not exist.", exception.Message);
    }

    [Fact]
    public async Task GetGamesByGenreIdAsync_WhenGenreExists_ReturnsMappedGames()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(ExistingGenreId))
            .ReturnsAsync(ExistingGenre1);

        var gameEntities = new List<GameEntity>
    {
        new() { Id = ExistingGameId, Name = "Game 1", Key = ExistingGameKey },
        new() { Id = ExistingGameId2, Name = "Game 2", Key = ExistingGameKey2 },
    };

        _unitOfWorkMock.Setup(x => x.GameGenreRepository.GetGamesByGenreIdAsync(ExistingGenreId))
            .ReturnsAsync(gameEntities);

        List<GameModel> gameModels = gameEntities
     .Select(g => new GameModel
     {
         Id = g.Id,
         Name = g.Name,
         Key = g.Key,
     })
     .ToList();

        _mapperMock.Setup(m => m.Map<List<GameModel>>(gameEntities))
            .Returns(gameModels);

        // Act
        var result = await _genreService.GetGamesByGenreIdAsync(ExistingGenreId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Collection(
            result,
            g => Assert.Equal("Game 1", g.Name),
            g => Assert.Equal("Game 2", g.Name));

        _unitOfWorkMock.Verify(x => x.GenreRepository.GetByIdAsync(ExistingGenreId), Times.Once);
        _unitOfWorkMock.Verify(x => x.GameGenreRepository.GetGamesByGenreIdAsync(ExistingGenreId), Times.Once);
        _mapperMock.Verify(m => m.Map<List<GameModel>>(gameEntities), Times.Once);
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