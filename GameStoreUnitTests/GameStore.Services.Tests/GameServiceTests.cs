using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;
using GameStore.Shared.FileSystem;
using Moq;
using Xunit;
using static GameStoreUnitTests.GameStore.Services.Tests.TestConstants;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class GameServiceTests
{
    private readonly GameService _gameService;
    private readonly Mock<IUnitOfWork<GameStoreContext>> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFileSystem> _fileSystemMock;

    public GameServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork<GameStoreContext>>();
        _mapperMock = new Mock<IMapper>();
        _fileSystemMock = new Mock<IFileSystem>();
        _gameService = new GameService(_unitOfWorkMock.Object, _mapperMock.Object, _fileSystemMock.Object);
    }

    [Fact]
    public async Task CreateGameAsync_WhenPlatformDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var missingPlatformId = NonExistingPlatformId;
        var gameModel = CreateGameModel(platforms: [missingPlatformId]);

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync((GameEntity)null);

        _unitOfWorkMock.Setup(x => x.PlatformRepository.GetAllAsync())
            .ReturnsAsync(ExistingPlatforms);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.CreateGameAsync(gameModel));

        // Assert
        Assert.Equal($"Platform with ID '{missingPlatformId}' was not found.", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WhenGenreDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var missingGenreId = NonExistingGenreId;
        var gameModel = CreateGameModel(
            platforms: ExistingPlatformIds,
            genres: [missingGenreId]);

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync((GameEntity)null);

        _unitOfWorkMock.Setup(x => x.PlatformRepository.GetAllAsync())
            .ReturnsAsync(ExistingPlatforms);

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetAllAsync())
            .ReturnsAsync(ExistingGenres);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.CreateGameAsync(gameModel));

        // Assert
        Assert.Equal($"Genre with ID '{missingGenreId}' was not found.", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WhenDataIsValid_ShouldCreateGame()
    {
        // Arrange
        var gameModel = CreateGameModel(
            platforms: ExistingPlatformIds,
            genres: ExistingGenreIds);

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync((GameEntity)null);

        _unitOfWorkMock.Setup(x => x.PlatformRepository.GetAllAsync())
            .ReturnsAsync(ExistingPlatforms);

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetAllAsync())
            .ReturnsAsync(ExistingGenres);

        var gameEntity = new GameEntity();
        _mapperMock.Setup(x => x.Map<GameEntity>(gameModel))
            .Returns(gameEntity);

        _unitOfWorkMock.Setup(x => x.GameRepository.AddAsync(gameEntity));
        _unitOfWorkMock.Setup(x => x.SaveAsync());

        // Act
        await _gameService.CreateGameAsync(gameModel);

        // Assert
        _unitOfWorkMock.Verify(x => x.GameRepository.AddAsync(gameEntity), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGameByKeyAsync_WhenGameDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync((GameEntity)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.GetGameByKeyAsync(NonExistingGameKey));

        // Assert
        Assert.Equal($"Game with Key '{NonExistingGameKey}' was not found.", exception.Message);
    }

    [Fact]
    public async Task GetGameByKeyAsync_WhenGameExists_ShouldReturnGame()
    {
        // Arrange
        var gameEntity = new GameEntity();
        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(gameEntity);

        var gameModel = new GameModel { Key = ExistingGameKey };
        _mapperMock.Setup(x => x.Map<GameModel>(gameEntity)).Returns(gameModel);

        // Act
        var result = await _gameService.GetGameByKeyAsync(ExistingGameKey);

        // Assert
        Assert.Equal(gameModel, result);
    }

    [Fact]
    public async Task UpdateGameAsync_WhenGameDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var gameModel = CreateGameModel();
        gameModel.Game.Id = NonExistingGameId;

        _unitOfWorkMock.Setup(x => x.GameRepository.GetByIdAsync(NonExistingGameId))
            .ReturnsAsync((GameEntity)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.UpdateGameAsync(gameModel));

        // Assert
        Assert.Equal($"Game with Id `{NonExistingGameId}` was not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateGameAsync_WhenGenreDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var missingGenreId = NonExistingGenreId;
        var gameModel = CreateGameModel(
            platforms: ExistingPlatformIds,
            genres: [missingGenreId]);
        gameModel.Game.Id = ExistingGameId;

        var existingGame = new GameEntity { Id = ExistingGameId };

        _unitOfWorkMock.Setup(x => x.GameRepository.GetByIdAsync(ExistingGameId))
            .ReturnsAsync(existingGame);

        _unitOfWorkMock.Setup(x => x.GameGenreRepository.DeleteByGameIdAsync(existingGame.Id))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GamePlatformRepository.DeleteByGameIdAsync(existingGame.Id))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetAllAsync())
            .ReturnsAsync(ExistingGenres);

        _unitOfWorkMock.Setup(x => x.PlatformRepository.GetAllAsync())
            .ReturnsAsync(ExistingPlatforms);

        _mapperMock.Setup(x => x.Map<GameEntity>(gameModel))
            .Returns(existingGame);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.UpdateGameAsync(gameModel));

        // Assert
        Assert.Equal($"Genre with Id `{missingGenreId}` was not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateGameAsync_WhenPlatformDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var missingPlatformId = NonExistingPlatformId;
        var gameModel = CreateGameModel(
            platforms: [missingPlatformId],
            genres: ExistingGenreIds);
        gameModel.Game.Id = ExistingGameId;

        var existingGame = new GameEntity { Id = ExistingGameId };

        _unitOfWorkMock.Setup(x => x.GameRepository.GetByIdAsync(ExistingGameId))
            .ReturnsAsync(existingGame);

        _unitOfWorkMock.Setup(x => x.GameGenreRepository.DeleteByGameIdAsync(existingGame.Id))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GamePlatformRepository.DeleteByGameIdAsync(existingGame.Id))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetAllAsync())
            .ReturnsAsync(ExistingGenres);

        _unitOfWorkMock.Setup(x => x.PlatformRepository.GetAllAsync())
            .ReturnsAsync(ExistingPlatforms);

        _mapperMock.Setup(x => x.Map<GameEntity>(gameModel))
            .Returns(existingGame);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.UpdateGameAsync(gameModel));

        // Assert
        Assert.Equal($"Platform with Id `{missingPlatformId}` was not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateGameAsync_WhenDataIsValid_ShouldUpdateGame()
    {
        // Arrange
        var gameModel = CreateGameModel(
            platforms: ExistingPlatformIds,
            genres: ExistingGenreIds);
        gameModel.Game.Id = ExistingGameId;

        var existingGame = new GameEntity { Id = ExistingGameId };

        _unitOfWorkMock.Setup(x => x.GameRepository.GetByIdAsync(ExistingGameId))
            .ReturnsAsync(existingGame);

        _unitOfWorkMock.Setup(x => x.GameGenreRepository.DeleteByGameIdAsync(existingGame.Id))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GamePlatformRepository.DeleteByGameIdAsync(existingGame.Id))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetAllAsync())
            .ReturnsAsync(ExistingGenres);

        _unitOfWorkMock.Setup(x => x.PlatformRepository.GetAllAsync())
            .ReturnsAsync(ExistingPlatforms);

        _mapperMock.Setup(x => x.Map<GameEntity>(gameModel))
            .Returns(existingGame);

        _unitOfWorkMock.Setup(x => x.GameGenreRepository.AddRangeAsync(It.IsAny<List<GameGenreEntity>>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GamePlatformRepository.AddRangeAsync(It.IsAny<List<GamePlatformEntity>>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GameRepository.UpdateAsync(existingGame))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _gameService.UpdateGameAsync(gameModel);

        // Assert
        _unitOfWorkMock.Verify(x => x.GameGenreRepository.AddRangeAsync(It.IsAny<List<GameGenreEntity>>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.GamePlatformRepository.AddRangeAsync(It.IsAny<List<GamePlatformEntity>>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.GameRepository.UpdateAsync(existingGame), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(NonExistingGameKey))
            .ReturnsAsync((GameEntity)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.DeleteGameAsync(NonExistingGameKey));

        // Assert
        Assert.Equal($"Game with Key '{NonExistingGameKey}' was not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameExists_ShouldDeleteGame()
    {
        // Arrange
        var existingGame = new GameEntity();

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(ExistingGameKey))
            .ReturnsAsync(existingGame);

        _unitOfWorkMock.Setup(x => x.GameRepository.DeleteAsync(existingGame))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _gameService.DeleteGameAsync(ExistingGameKey);

        // Assert
        _unitOfWorkMock.Verify(x => x.GameRepository.DeleteAsync(existingGame), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGameFilePathAsync_WhenGameDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(NonExistingGameKey))
            .ReturnsAsync((GameEntity)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.GetGameByKeyAsync(NonExistingGameKey));

        // Assert
        Assert.Equal($"Game with Key '{NonExistingGameKey}' was not found.", exception.Message);
    }

    [Fact]
    public async Task GetGameFilePathAsync_WhenGameFileEntityDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        var gameEntity = new GameEntity();

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(ExistingGameKey))
            .ReturnsAsync(gameEntity);

        _unitOfWorkMock.Setup(x => x.GameFileRepository.GetGameFileByKeyAsync(ExistingGameKey))
        .ReturnsAsync((GameFileEntity)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.GetGameFilePathAsync(ExistingGameKey));

        // Assert
        Assert.Equal($"Game file for game with Key '{ExistingGameKey}' was not found.", exception.Message);
    }

    [Fact]
    public async Task GetGameFilePathAsync_WhenPhysicalFileDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        string fakeFilePath = "/non-existing-file-path.zip";

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(ExistingGameKey))
        .ReturnsAsync(new GameEntity());

        _unitOfWorkMock.Setup(x => x.GameFileRepository.GetGameFileByKeyAsync(ExistingGameKey)).
            ReturnsAsync(new GameFileEntity
            {
                FilePath = fakeFilePath,
            });

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.GetGameFilePathAsync(ExistingGameKey));

        // Assert
        Assert.Equal(
            $"Physical file with path '{fakeFilePath}' not found on server.",
            exception.Message);
    }

    [Fact]
    public async Task GetGameFilePathAsync_WhenFileExists_ShouldReturnFilePath()
    {
        // Arrange
        string filePath = "tempPath";

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(ExistingGameKey))
        .ReturnsAsync(new GameEntity());

        _unitOfWorkMock.Setup(x => x.GameFileRepository.GetGameFileByKeyAsync(ExistingGameKey)).
            ReturnsAsync(new GameFileEntity
            {
                FilePath = filePath,
            });

        _fileSystemMock.Setup(x => x.FileExists(filePath))
            .Returns(true);

        // Act
        string result = await _gameService.GetGameFilePathAsync(ExistingGameKey);

        // Assert
        Assert.Equal(filePath, result);
    }

    [Fact]
    public async Task GetAllGamesAsync_WhenGamesExist_ShouldReturnMappedGames()
    {
        List<GameEntity> gameEntities =
        [
            new GameEntity { Id = ExistingGameId, Name = "Game 1", Key = ExistingGameKey },
            new GameEntity { Id = ExistingGameId2, Name = "Game 2", Key = ExistingGameKey2 },
        ];

        _unitOfWorkMock.Setup(x => x.GameRepository.GetAllAsync())
            .ReturnsAsync(gameEntities);

        List<GameModel> mappedGames =
        [
            new GameModel { Id = ExistingGameId, Name = "Game 1", Key = ExistingGameKey },
            new GameModel { Id = ExistingGameId2, Name = "Game 2", Key = ExistingGameKey2 },
        ];

        _mapperMock.Setup(x => x.Map<IList<GameModel>>(gameEntities))
            .Returns(mappedGames);

        // Act
        IList<GameModel> result = await _gameService.GetAllGamesAsync();

        // Assert
        Assert.Equal(mappedGames, result);
    }

    [Fact]
    public async Task GetGenresByGameKeyAsync_WhenGameDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        List<GenreEntity> emptyGenreEntities = [];

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGenresByGameKeyAsync(NonExistingGameKey))
            .ReturnsAsync(emptyGenreEntities);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                          _gameService.GetGenresByGameKeyAsync(NonExistingGameKey));

        // Assert
        Assert.Equal($"Game with Key '{NonExistingGameKey}' was not found or has no genres.", exception.Message);
    }

    [Fact]
    public async Task GetGenresByGameKeyAsync_WhenValidData_ShouldReturnMappedGenres()
    {
        // Arrange
        List<GenreEntity> genreEntities =
        [
            new GenreEntity { Id = ExistingGenreId, Name = "Action" },
            new GenreEntity { Id = AnotherExistingGenreId, Name = "Adventure" },
        ];

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGenresByGameKeyAsync(ExistingGameKey))
            .ReturnsAsync(genreEntities);

        List<GenreModel> mappedGenres =
        [
            new GenreModel { Id = genreEntities[0].Id, Name = "Action" },
            new GenreModel { Id = genreEntities[1].Id, Name = "Adventure" },
        ];

        _mapperMock.Setup(x => x.Map<IList<GenreModel>>(genreEntities))
            .Returns(mappedGenres);

        // Act
        IList<GenreModel> result = await _gameService.GetGenresByGameKeyAsync(ExistingGameKey);

        // Assert
        Assert.Equal(mappedGenres, result);
    }

    [Fact]
    public async Task GetPlatformsByGameKeyAsync_WhenGameDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        List<PlatformEntity> emptyPlatformEntities = [];

        _unitOfWorkMock.Setup(x => x.GameRepository.GetPlatformsByGameKeyAsync(NonExistingGameKey))
            .ReturnsAsync(emptyPlatformEntities);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                                 _gameService.GetPlatformsByGameKeyAsync(NonExistingGameKey));

        // Assert
        Assert.Equal($"Game with Key '{NonExistingGameKey}' was not found or has no platforms.", exception.Message);
    }

    [Fact]
    public async Task GetPlatformsByGameKeyAsync_WhenValidData_ShouldReturnMappedPlatforms()
    {
        // Arrange
        List<PlatformEntity> platformEntities =
        [
            new PlatformEntity { Id = ExistingPlatformId, Type = "PC" },
            new PlatformEntity { Id = AnotherExistingPlatformId, Type = "Xbox" },
        ];

        _unitOfWorkMock.Setup(x => x.GameRepository.GetPlatformsByGameKeyAsync(ExistingGameKey))
            .ReturnsAsync(platformEntities);

        List<PlatformModel> mappedPlatforms =
            [
            new PlatformModel { Id = platformEntities[0].Id, Type = "PC" },
                new PlatformModel { Id = platformEntities[1].Id, Type = "Xbox" },
            ];

        _mapperMock.Setup(x => x.Map<IList<PlatformModel>>(platformEntities))
            .Returns(mappedPlatforms);

        // Act
        IList<PlatformModel> result = await _gameService.GetPlatformsByGameKeyAsync(ExistingGameKey);

        // Assert
        Assert.Equal(mappedPlatforms, result);
    }

    private static CreateGameModel CreateGameModel(
      string key = "",
      string name = "",
      IEnumerable<Guid>? platforms = null,
      IEnumerable<Guid>? genres = null)
    {
        return new CreateGameModel
        {
            Game = new GameModel
            {
                Key = key,
                Name = name,
            },
            Platforms = platforms?.ToList() ?? [],
            Genres = genres?.ToList() ?? [],
        };
    }
}
