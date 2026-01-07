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

    [Fact]
    public async Task CreateGenreAsyncThrowsArgumentExceptionWhenParentGenreDoesNotExist()
    {
        var genreModel = new GenreModel
        {
            Id = Guid.NewGuid(),
            Name = "Action",
            ParentGenreId = Guid.NewGuid(),
        };

        _unitOfWorkMock
        .Setup(x => x.GenreRepository.GetByIdAsync(genreModel.ParentGenreId.Value))
        .ReturnsAsync((GenreEntity?)null);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _genreService.CreateGenreAsync(genreModel));

        Assert.Equal($"Parent genre with Id {genreModel.ParentGenreId} does not exist.", exception.Message);
    }

    [Fact]
    public async Task CreateGenreAsyncAddsGenreToRepositoryAndSavesChanges()
    {
        var genreModel = new GenreModel
        {
            Id = Guid.NewGuid(),
            Name = "Action",
            ParentGenreId = null,
        };

        GenreEntity genreEntity = new()
        {
            Id = genreModel.Id,
            Name = genreModel.Name,
            ParentGenreId = genreModel.ParentGenreId,
        };

        _mapperMock.Setup(m => m.Map<GenreEntity>(genreModel))
            .Returns(genreEntity);

        _unitOfWorkMock.Setup(uow => uow.GenreRepository.AddAsync(genreEntity))
            .Returns(Task.CompletedTask);

        await _genreService.CreateGenreAsync(genreModel);

        _unitOfWorkMock.Verify(uow => uow.GenreRepository.AddAsync(genreEntity), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGenreByIdAsyncGenreIdDoesNotExistThrowNotFoundException()
    {
        Guid genreId = Guid.NewGuid();
        GenreEntity nullGameEntity = null;

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(genreId)).ReturnsAsync(nullGameEntity);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
        () => _genreService.GetGenreByIdAsync(genreId));

        Assert.Equal($"Genre with Id {genreId} does not exist.", exception.Message);

        _unitOfWorkMock.Verify(
       u => u.GenreRepository.GetByIdAsync(genreId),
       Times.Once);
    }

    [Fact]
    public async Task GetGenreByIdAsyncReturnsGenreModelWhenGenreExists()
    {
        var genreId = Guid.NewGuid();
        var genreEntity = new GenreEntity
        {
            Id = genreId,
            Name = "Action",
        };

        _unitOfWorkMock.Setup(uow => uow.GenreRepository.GetByIdAsync(genreId))
            .ReturnsAsync(genreEntity);

        var genreModel = new GenreModel
        {
            Id = genreId,
            Name = "Action",
        };

        _mapperMock.Setup(m => m.Map<GenreModel>(genreEntity))
            .Returns(genreModel);

        var result = await _genreService.GetGenreByIdAsync(genreId);

        Assert.Equal(genreId, result.Id);
        Assert.Equal("Action", result.Name);
    }

    [Fact]
    public async Task GetAllGenresAsyncReturnsListOfGenreModels()
    {
        var genreEntities = new List<GenreEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Action" },
            new() { Id = Guid.NewGuid(), Name = "Adventure" },
        };

        _unitOfWorkMock.Setup(uow => uow.GenreRepository.GetAllAsync())
            .ReturnsAsync(genreEntities);

        var genreModels = new List<GenreModel>
        {
            new() { Id = genreEntities[0].Id, Name = "Action" },
            new() { Id = genreEntities[1].Id, Name = "Adventure" },
        };

        _mapperMock.Setup(m => m.Map<List<GenreModel>>(genreEntities))
            .Returns(genreModels);

        var result = await _genreService.GetAllGenresAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Action", result[0].Name);
        Assert.Equal("Adventure", result[1].Name);
    }

    [Fact]
    public async Task GetGenresByParentGenreIdAsyncParentGenreIdDoesNotExistThrowNotFoundException()
    {
        Guid parentGenreId = Guid.NewGuid();
        GenreEntity nullGenreEntity = null;

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(parentGenreId))
            .ReturnsAsync(nullGenreEntity);

        var exeption = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.GetGenresByParentGenreIdAsync(parentGenreId));

        Assert.Equal($"Parent genre with Id {parentGenreId} does not exist.", exeption.Message);

        _unitOfWorkMock.Verify(
            x => x.GenreRepository.GetByIdAsync(parentGenreId),
            Times.Once);
    }

    [Fact]
    public async Task GetGenresByParentGenreIdAsyncWhenParentGenreExistsReturnsGenreModels()
    {
        Guid parentGenreId = Guid.NewGuid();
        GenreEntity genreEntity = new();

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(parentGenreId))
            .ReturnsAsync(genreEntity);

        List<GenreEntity> genreEntities =
        [
        new GenreEntity { Id = Guid.NewGuid(), Name = "Child 1", ParentGenreId = parentGenreId },
            new GenreEntity { Id = Guid.NewGuid(), Name = "Child 2", ParentGenreId = parentGenreId },
        ];

        _unitOfWorkMock
            .Setup(u => u.GenreRepository.FindAsync(It.IsAny<Expression<Func<GenreEntity, bool>>>()))
            .ReturnsAsync(genreEntities);

        List<GenreModel> genreModels = genreEntities.Select(g => new GenreModel
        {
            Id = g.Id,
            Name = g.Name,
        }).ToList();

        _mapperMock.Setup(m => m.Map<List<GenreModel>>(genreEntities))
            .Returns(genreModels);

        var result = await _genreService.GetGenresByParentGenreIdAsync(parentGenreId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Collection(
            result,
            item => Assert.Equal("Child 1", item.Name),
            item => Assert.Equal("Child 2", item.Name));

        _unitOfWorkMock.Verify(u => u.GenreRepository.GetByIdAsync(parentGenreId), Times.Once);
        _unitOfWorkMock.Verify(
            u => u.GenreRepository.FindAsync(
        It.IsAny<Expression<Func<GenreEntity, bool>>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateGenreAsyncGenreIdDoesNotExistThrowNotFoundException()
    {
        Guid genreId = Guid.NewGuid();

        GenreModel genreModel = new()
        {
            Id = genreId,
            Name = "Action",
        };

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(genreId))
        .ReturnsAsync((GenreEntity?)null);

        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.UpdateGenreAsync(genreModel));

        Assert.Equal($"Genre with Id {genreModel.Id} does not exist.", exception.Message);
    }

    [Fact]
    public async Task UpdateGenreAsyncParentGenreIdDoesNotExistThrowNotFoundException()
    {
        Guid genreId = Guid.NewGuid();
        Guid parentGenreId = Guid.NewGuid();

        GenreModel genreModel = new()
        {
            Id = genreId,
            Name = "Action",
            ParentGenreId = parentGenreId,
        };

        GenreEntity genreEntity = new()
        {
            Id = genreId,
            ParentGenreId = Guid.NewGuid(),
        };

        GenreEntity nullGenreEntity = null;

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(genreId))
            .ReturnsAsync(genreEntity);

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(parentGenreId))
            .ReturnsAsync(nullGenreEntity);

        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.UpdateGenreAsync(genreModel));

        Assert.Equal(
      $"Parent genre with Id {parentGenreId} does not exist.",
      exception.Message);

        _unitOfWorkMock.Verify(
            x => x.GenreRepository.GetByIdAsync(parentGenreId),
            Times.Once);
    }

    [Fact]
    public async Task UpdateGenreAsyncWhenDataIsValidUpdatesGenreAndSaves()
    {
        Guid genreId = Guid.NewGuid();
        Guid parentGenreId = Guid.NewGuid();

        GenreModel genreModel = new()
        {
            Id = genreId,
            Name = "Action",
            ParentGenreId = parentGenreId,
        };

        GenreEntity existingGenre = new()
        {
            Id = genreId,
            Name = "Old Name",
            ParentGenreId = parentGenreId,
        };

        GenreEntity parentGenre = new()
        {
            Id = parentGenreId,
            Name = "Parent",
        };

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(genreId))
            .ReturnsAsync(existingGenre);

        _unitOfWorkMock
       .Setup(x => x.GenreRepository.GetByIdAsync(parentGenreId))
       .ReturnsAsync(parentGenre);

        _mapperMock
        .Setup(m => m.Map(genreModel, existingGenre));

        _unitOfWorkMock
       .Setup(x => x.GenreRepository.UpdateAsync(existingGenre))
       .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveAsync())
            .Returns(Task.CompletedTask);

        await _genreService.UpdateGenreAsync(genreModel);

        _unitOfWorkMock.Verify(
           x => x.GenreRepository.GetByIdAsync(It.IsAny<Guid>()),
           Times.Exactly(2));

        _mapperMock.Verify(
            m => m.Map(genreModel, existingGenre),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.GenreRepository.UpdateAsync(existingGenre),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveAsync(),
            Times.Once);
    }

    [Fact]
    public async Task DeleteGenreAsyncGenreIdDoesNotExistThrowNotFoundException()
    {
        Guid genreId = Guid.NewGuid();

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(genreId))
            .ReturnsAsync((GenreEntity?)null);

        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.DeleteGenreAsync(genreId));

        Assert.Equal($"Genre with Id {genreId} does not exist.", exception.Message);
    }

    [Fact]
    public async Task DeleteGenreAsyncWhenDataIsValidDeleteGenreAndSaves()
    {
        Guid genreId = Guid.NewGuid();

        GenreEntity genre = new()
        {
            Id = genreId,
            Name = "Parent Genre",
        };

        List<GenreEntity> subGenres =
       [
        new GenreEntity { Id = Guid.NewGuid(), Name = "Child 1", ParentGenreId = genreId },
           new GenreEntity { Id = Guid.NewGuid(), Name = "Child 2", ParentGenreId = genreId },
       ];

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(genreId))
            .ReturnsAsync(genre);

        _unitOfWorkMock
      .Setup(x => x.GenreRepository.FindAsync(
          It.IsAny<Expression<Func<GenreEntity, bool>>>()))
      .ReturnsAsync(subGenres);

        _unitOfWorkMock
      .Setup(x => x.GenreRepository.DeleteAsync(genre))
      .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveAsync())
            .Returns(Task.CompletedTask);

        await _genreService.DeleteGenreAsync(genreId);

        Assert.All(subGenres, sg => Assert.Null(sg.ParentGenreId));

        _unitOfWorkMock.Verify(
            x => x.GenreRepository.DeleteAsync(genre),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetGamesByGenreIdAsyncWhenGenreDoesNotExistThrowsNotFoundException()
    {
        Guid genreId = Guid.NewGuid();
        GenreEntity nullGenreEntity = null;

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(genreId))
            .ReturnsAsync(nullGenreEntity);

        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _genreService.GetGamesByGenreIdAsync(genreId));

        Assert.Equal($"Genre with id '{genreId}' does not exist.", exception.Message);
    }

    [Fact]
    public async Task GetGamesByGenreIdAsyncWhenGenreExistsReturnsMappedGames()
    {
        Guid genreId = Guid.NewGuid();

        GenreEntity genre = new()
        {
            Id = genreId,
        };

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetByIdAsync(genreId))
            .ReturnsAsync(genre);

        List<GameEntity> gameEntities =
    [
        new GameEntity { GameId = Guid.NewGuid(), GameName = "Game 1", GameKey = "game-1" },
        new GameEntity { GameId = Guid.NewGuid(), GameName = "Game 2", GameKey = "game-2" },
    ];

        _unitOfWorkMock
            .Setup(x => x.GameGenreRepository.GetGamesByGenreIdAsync(genreId))
            .ReturnsAsync(gameEntities);

        List<GameModel> gameModels =
    [
        new GameModel { Id = gameEntities[0].GameId, Name = "Game 1", Key = "game-1" },
        new GameModel { Id = gameEntities[1].GameId, Name = "Game 2", Key = "game-2" },
    ];

        _mapperMock
            .Setup(m => m.Map<List<GameModel>>(gameEntities))
            .Returns(gameModels);

        var result = await _genreService.GetGamesByGenreIdAsync(genreId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            g => Assert.Equal("Game 1", g.Name),
            g => Assert.Equal("Game 2", g.Name));

        _unitOfWorkMock.Verify(x => x.GenreRepository.GetByIdAsync(genreId), Times.Once);
        _unitOfWorkMock.Verify(x => x.GameGenreRepository.GetGamesByGenreIdAsync(genreId), Times.Once);
        _mapperMock.Verify(m => m.Map<List<GameModel>>(gameEntities), Times.Once);
    }
}
