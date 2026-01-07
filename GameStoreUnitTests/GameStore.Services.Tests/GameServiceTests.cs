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

public class GameServiceTests
{
    private readonly GameService _gameService;
    private readonly Mock<IUnitOfWork<GameStoreContext>> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;

    public GameServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork<GameStoreContext>>();
        _mapperMock = new Mock<IMapper>();
        _gameService = new GameService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task CreateGameAsyncPlatformNotExisthrowNotFoundException()
    {
        List<Guid> platformIds =
        [
            Guid.Parse("10000000-0000-0000-0000-000000000002"),
        ];
        FullGameModel gameModel = CreateGameModel(platforms: platformIds);

        GameEntity nullGameEntity = null;

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(nullGameEntity);

        PlatformEntity platformEntity1 = new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Type = "PC",
        };

        PlatformEntity platformEntity2 = new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            Type = "Xbox",
        };

        IEnumerable<PlatformEntity> platformEntities = [platformEntity1, platformEntity2];

        _unitOfWorkMock.Setup(x => x.PlatformRepository.GetAllAsync())
            .ReturnsAsync(platformEntities);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.CreateGameAsync(gameModel));

        Assert.Equal("Platform with ID '10000000-0000-0000-0000-000000000002' was not found.", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsyncGenreNotExisthrowNotFoundException()
    {
        FullGameModel fullGameModel = CreateGameModel(
                               platforms: [Guid.Parse("10000000-0000-0000-0000-000000000001")],
                               genres: [Guid.Parse("30000000-0000-0000-0000-000000000003")]);

        GameEntity nullGameEntity = null;

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(nullGameEntity);

        PlatformEntity platformEntity1 = new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Type = "PC",
        };

        PlatformEntity platformEntity2 = new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            Type = "Xbox",
        };

        IEnumerable<PlatformEntity> platformEntities = [platformEntity1, platformEntity2];

        _unitOfWorkMock.Setup(x => x.PlatformRepository.GetAllAsync())
            .ReturnsAsync(platformEntities);

        IEnumerable<GenreEntity> genreEntities = [
            new()
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Action",
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Name = "Adventure",
            },
        ];

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetAllAsync())
            .ReturnsAsync(genreEntities);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.CreateGameAsync(fullGameModel));

        Assert.Equal("Genre with ID '30000000-0000-0000-0000-000000000003' was not found.", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsyncGoodDataSuccessCreation()
    {
        FullGameModel fullgameModel = CreateGameModel(
            platforms: [Guid.Parse("10000000-0000-0000-0000-000000000001")],
            genres: [Guid.Parse("30000000-0000-0000-0000-000000000003")]);

        GameEntity nullGameEntity = null;

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(nullGameEntity);

        PlatformEntity platformEntity1 = new()
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Type = "PC",
        };

        PlatformEntity platformEntity2 = new()
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            Type = "Xbox",
        };

        IEnumerable<PlatformEntity> platformEntities = [platformEntity1, platformEntity2];

        _unitOfWorkMock.Setup(x => x.PlatformRepository.GetAllAsync())
            .ReturnsAsync(platformEntities);

        IEnumerable<GenreEntity> genreEntities = [
            new()
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                Name = "Action",
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Name = "Adventure",
            },
        ];

        _unitOfWorkMock.Setup(x => x.GenreRepository.GetAllAsync())
            .ReturnsAsync(genreEntities);

        GameEntity gameEntity = new();
        _mapperMock.Setup(x => x.Map<GameEntity>(fullgameModel))
            .Returns(gameEntity);

        _unitOfWorkMock.Setup(x => x.GameRepository.AddAsync(gameEntity));
        _unitOfWorkMock.Setup(x => x.SaveAsync());

        await _gameService.CreateGameAsync(fullgameModel);

        _unitOfWorkMock.Verify(x => x.GameRepository.AddAsync(gameEntity), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGameByKeyAsyncGameNotExisthrowNotFoundException()
    {
        GameEntity nullGameEntity = null;

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(nullGameEntity);

        string key = "non-existing-key";

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                   _gameService.GetGameByKeyAsync(key));

        Assert.Equal($"Game with Key '{key}' was not found.", exception.Message);
    }

    [Fact]
    public async Task GetGameByKeyAsyncGameExistsSuccessfulGetting()
    {
        GameEntity gameEntity = new();

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(gameEntity);

        string key = "existing-key";

        GameModel gameModel = new()
        {
            Key = key,
        };
        _mapperMock.Setup(x => x.Map<GameModel>(gameEntity))
            .Returns(gameModel);

        GameModel result = await _gameService.GetGameByKeyAsync(key);

        Assert.Equal(gameModel, result);
    }

    [Fact]
    public async Task UpdateGameAsyncGameIdNotExisthrowNotFoundException()
    {
        Guid gameId = Guid.Parse("10000000-0000-0700-0000-000000009991");

        FullGameModel gameModel = CreateGameModel();

        gameModel.Game.Id = gameId;

        GameEntity nullGameEntity = null;

        _unitOfWorkMock.Setup(x => x.GameRepository.GetByIdAsync(gameId))
            .ReturnsAsync(nullGameEntity);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                   _gameService.UpdateGameAsync(gameModel));

        Assert.Equal($"Game with Id `{gameModel.Game.Id}` was not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateGameAsyncGenreNotExisthrowNotFoundException()
    {
        var gameId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var missingGenreId = Guid.Parse("11111111-1111-1111-1111-111111155555");
        FullGameModel gameModel = CreateGameModel(
                       genres: [missingGenreId]);

        gameModel.Game.Id = gameId;

        GameEntity existingGameEntity = new()
        {
            GameId = gameId,
        };

        _unitOfWorkMock.Setup(x => x.GameRepository.GetByIdAsync(gameId))
            .ReturnsAsync(existingGameEntity);

        _unitOfWorkMock.Setup(x => x.GameGenreRepository.
        DeleteByGameIdAsync(existingGameEntity.GameId))
        .Returns(Task.CompletedTask);

        _mapperMock
       .Setup(x => x.Map<GameEntity>(gameModel))
       .Returns(existingGameEntity);

        _unitOfWorkMock.Setup(x => x.GenreRepository.CheckIfExistAsync(missingGenreId))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                          _gameService.UpdateGameAsync(gameModel));

        Assert.Equal($"Genre with Id `{missingGenreId}` was not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateGameAsyncPlatformNotExisthrowNotFoundException()
    {
        var gameId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var genreId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var missingPlatformId = Guid.Parse("22222222-2222-2222-2222-222222225555");

        FullGameModel gameModel = CreateGameModel(
            platforms: [missingPlatformId],
            genres: [genreId]);
        gameModel.Game.Id = gameId;

        GameEntity existingGameEntity = new()
        {
            GameId = gameId,
        };

        _unitOfWorkMock.Setup(x => x.GameRepository.GetByIdAsync(gameId))
            .ReturnsAsync(existingGameEntity);

        _mapperMock.Setup(x => x.Map<GameEntity>(gameModel))
        .Returns(existingGameEntity);

        _unitOfWorkMock.Setup(x => x.GameGenreRepository.DeleteByGameIdAsync(existingGameEntity.GameId))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GenreRepository.CheckIfExistAsync(genreId))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.GameGenreRepository.AddAsync(It.IsAny<GameGenreEntity>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GamePlatformRepository.DeleteByGameIdAsync(existingGameEntity.GameId))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.PlatformRepository.CheckIfExistAsync(missingPlatformId))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                                 _gameService.UpdateGameAsync(gameModel));

        Assert.Equal($"Platform with Id `{missingPlatformId}` was not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateGameAsyncValidDataSuccessUpdate()
    {
        var gameId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var genreId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var platformId = Guid.Parse("AAAAAAAA-0000-0000-0000-000000000001");

        FullGameModel gameModel = CreateGameModel(
                       platforms: [platformId],
                       genres: [genreId]);
        gameModel.Game.Id = gameId;

        GameEntity existingGameEntity = new()
        {
            GameId = gameId,
        };

        _unitOfWorkMock.Setup(x => x.GameRepository.GetByIdAsync(gameId))
            .ReturnsAsync(existingGameEntity);

        _mapperMock.Setup(x => x.Map<GameEntity>(gameModel))
        .Returns(existingGameEntity);

        _unitOfWorkMock.Setup(x => x.GameGenreRepository.DeleteByGameIdAsync(existingGameEntity.GameId))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GenreRepository.CheckIfExistAsync(genreId))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.GameGenreRepository.AddAsync(It.IsAny<GameGenreEntity>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.GamePlatformRepository.DeleteByGameIdAsync(existingGameEntity.GameId))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.PlatformRepository.CheckIfExistAsync(platformId))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.GamePlatformRepository.AddAsync(It.IsAny<GamePlatformEntity>()))
            .Returns(Task.CompletedTask);

        await _gameService.UpdateGameAsync(gameModel);

        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteGameAsyncGameNotExisthrowNotFoundException()
    {
        string gameKey = "non-existing-key";
        GameEntity nullGameEntity = null;

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(gameKey))
            .ReturnsAsync(nullGameEntity);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                          _gameService.DeleteGameAsync(gameKey));

        Assert.Equal($"Game with Key '{gameKey}' was not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteGameAsyncValidDataSuccessDelete()
    {
        string gameKey = "existing-key";
        GameEntity existingGameEntity = new();

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(gameKey))
            .ReturnsAsync(existingGameEntity);

        _unitOfWorkMock.Setup(x => x.GameRepository.DeleteAsync(existingGameEntity))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
         .Setup(x => x.SaveAsync())
         .Returns(Task.CompletedTask);

        await _gameService.DeleteGameAsync(gameKey);

        _unitOfWorkMock.Verify(
            x => x.GameRepository.DeleteAsync(existingGameEntity),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetGameFilePathAsyncGameKeyNotExisthrowNotFoundException()
    {
        string gameKey = "non-existing-key";
        GameEntity nullGameEntity = null;

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(gameKey))
            .ReturnsAsync(nullGameEntity);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                                 _gameService.GetGameFilePathAsync(gameKey));

        Assert.Equal($"Game with Key '{gameKey}' was not found.", exception.Message);
    }

    [Fact]
    public async Task GetGameFilePathAsyncGameFileNotExisthrowNotFoundException()
    {
        string gameKey = "existing-key";

        _unitOfWorkMock
       .Setup(x => x.GameRepository.GetGameByKeyAsync(gameKey))
       .ReturnsAsync(new GameEntity());

        _unitOfWorkMock
            .Setup(x => x.GameFileRepository.GetGameFileByKeyAsync(gameKey))
            .ReturnsAsync((GameFileEntity)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.GetGameFilePathAsync(gameKey));

        Assert.Equal("Game file not found.", exception.Message);
    }

    [Fact]
    public async Task GetGameFilePathAsyncPhysicalFileMissingThrowsNotFoundException()
    {
        string gameKey = "existing-key";
        string filePath = "/files/game.zip";

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(gameKey))
            .ReturnsAsync(new GameEntity());

        _unitOfWorkMock.Setup(x => x.GameFileRepository.GetGameFileByKeyAsync(gameKey)).
            ReturnsAsync(new GameFileEntity
            {
                FilePath = filePath,
            });

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _gameService.GetGameFilePathAsync(gameKey));

        Assert.Equal(
            $"Physical file with path '{filePath}' not found on server.",
            exception.Message);
    }

    [Fact]
    public async Task GetGameFilePathAsyncValidDataReturnsFilePath()
    {
        string gameKey = "existing-key";
        string filePath = Path.GetTempFileName();

        try
        {
            _unitOfWorkMock.Setup(x => x.GameRepository.GetGameByKeyAsync(gameKey))
            .ReturnsAsync(new GameEntity());

            _unitOfWorkMock.Setup(x => x.GameFileRepository.GetGameFileByKeyAsync(gameKey)).
                ReturnsAsync(new GameFileEntity
                {
                    FilePath = filePath,
                });

            string result = await _gameService.GetGameFilePathAsync(gameKey);

            Assert.Equal(filePath, result);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task GetAllGamesAsyncReturnsMappedGames()
    {
        Guid id = Guid.NewGuid();
        Guid anotherId = Guid.NewGuid();
        List<GameEntity> gameEntities =
        [
            new GameEntity { GameId = id, GameName = "Game 1", GameKey = "game-1" },
            new GameEntity { GameId = anotherId, GameName = "Game 2", GameKey = "game-2" },
        ];

        _unitOfWorkMock.Setup(x => x.GameRepository.GetAllAsync())
            .ReturnsAsync(gameEntities);

        List<GameModel> mappedGames =
        [
            new GameModel { Id = id, Name = "Game 1", Key = "game-1" },
            new GameModel { Id = anotherId, Name = "Game 2", Key = "game-2" },
        ];

        _mapperMock.Setup(x => x.Map<IList<GameModel>>(gameEntities))
            .Returns(mappedGames);

        IList<GameModel> result = await _gameService.GetAllGamesAsync();

        Assert.Equal(mappedGames, result);
    }

    [Fact]
    public async Task GetGenresByGameKeyAsyncGameKeyNotExisthrowNotFoundException()
    {
        string gameKey = "non-existing-key";
        List<GenreEntity> emptyGenreEntities = [];

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGenresByGameKeyAsync(gameKey))
            .ReturnsAsync(emptyGenreEntities);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                          _gameService.GetGenresByGameKeyAsync(gameKey));

        Assert.Equal($"Game with Key '{gameKey}' was not found or has no genres.", exception.Message);
    }

    [Fact]
    public async Task GetGenresByGameKeyAsyncValidDataReturnsMappedGenres()
    {
        string gameKey = "existing-key";
        List<GenreEntity> genreEntities =
        [
            new GenreEntity { Id = Guid.NewGuid(), Name = "Action" },
            new GenreEntity { Id = Guid.NewGuid(), Name = "Adventure" },
        ];

        _unitOfWorkMock.Setup(x => x.GameRepository.GetGenresByGameKeyAsync(gameKey))
            .ReturnsAsync(genreEntities);

        List<GenreModel> mappedGenres =
        [
            new GenreModel { Id = genreEntities[0].Id, Name = "Action" },
            new GenreModel { Id = genreEntities[1].Id, Name = "Adventure" },
        ];

        _mapperMock.Setup(x => x.Map<IList<GenreModel>>(genreEntities))
            .Returns(mappedGenres);

        IList<GenreModel> result = await _gameService.GetGenresByGameKeyAsync(gameKey);

        Assert.Equal(mappedGenres, result);
    }

    [Fact]
    public async Task GetPlatformsByGameKeyAsyncGameKeyNotExisthrowNotFoundException()
    {
        string gameKey = "non-existing-key";
        List<PlatformEntity> emptyPlatformEntities = [];

        _unitOfWorkMock.Setup(x => x.GameRepository.GetPlatformsByGameKeyAsync(gameKey))
            .ReturnsAsync(emptyPlatformEntities);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                                 _gameService.GetPlatformsByGameKeyAsync(gameKey));

        Assert.Equal($"Game with Key '{gameKey}' was not found or has no platforms.", exception.Message);
    }

    [Fact]
    public async Task GetPlatformsByGameKeyAsyncValidDataReturnsMappedGenres()
    {
        string gameKey = "existing-key";
        List<PlatformEntity> platformEntities =
        [
            new PlatformEntity { Id = Guid.NewGuid(), Type = "PC" },
            new PlatformEntity { Id = Guid.NewGuid(), Type = "Xbox" },
        ];

        _unitOfWorkMock.Setup(x => x.GameRepository.GetPlatformsByGameKeyAsync(gameKey))
            .ReturnsAsync(platformEntities);

        List<PlatformModel> mappedPlatforms =
            [
            new PlatformModel { Id = platformEntities[0].Id, Type = "PC" },
                new PlatformModel { Id = platformEntities[1].Id, Type = "Xbox" },
            ];

        _mapperMock.Setup(x => x.Map<IList<PlatformModel>>(platformEntities))
            .Returns(mappedPlatforms);

        IList<PlatformModel> result = await _gameService.GetPlatformsByGameKeyAsync(gameKey);

        Assert.Equal(mappedPlatforms, result);
    }

    private static FullGameModel CreateGameModel(
      string key = "",
      string name = "",
      IEnumerable<Guid>? platforms = null,
      IEnumerable<Guid>? genres = null)
    {
        return new FullGameModel
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
