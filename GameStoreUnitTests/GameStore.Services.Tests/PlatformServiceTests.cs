using System.Linq.Expressions;
using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using FluentAssertions;
using GameStore.Shared.Exceptions;
using Moq;
using Xunit;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class PlatformServiceTests
{
    private readonly PlatformService _platformService;
    private readonly Mock<IUnitOfWork<GameStoreContext>> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;

    public PlatformServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork<GameStoreContext>>();
        _mapperMock = new Mock<IMapper>();
        _platformService = new PlatformService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task CreatePlatformAsyncShouldThrowArgumentExceptionWhenPlatformTypeExists()
    {
        var existingPlatformType = "PC";
        var existingPlatformEntity = new PlatformEntity
        {
            Id = Guid.NewGuid(),
            Type = existingPlatformType,
        };

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository
                   .FindAsync(It.IsAny<Expression<Func<PlatformEntity, bool>>>()))
            .ReturnsAsync([existingPlatformEntity]);

        var exeption = await Assert.ThrowsAsync<ArgumentException>(() => _platformService.CreatePlatformAsync(existingPlatformType));

        Assert.Equal($"Platform with type '{existingPlatformType}' already exists.", exeption.Message);

        _unitOfWorkMock.Verify(
            uow => uow.PlatformRepository.FindAsync(
                It.IsAny<Expression<Func<PlatformEntity, bool>>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePlatformAsyncShouldCreatePlatformWhenPlatformTypeNotExists()
    {
        var newPlatformType = "Xbox";
        List<PlatformEntity> platformEntities = [];
        _unitOfWorkMock.Setup(uow => uow.PlatformRepository
                          .FindAsync(It.IsAny<Expression<Func<PlatformEntity, bool>>>()))
            .ReturnsAsync(platformEntities);

        _mapperMock.Setup(m => m.Map<PlatformEntity>(It.IsAny<PlatformModel>()))
            .Returns((PlatformModel model) => new PlatformEntity
            {
                Id = Guid.NewGuid(),
                Type = model.Type,
            });

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.AddAsync(It.IsAny<PlatformEntity>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(uow => uow.SaveAsync())
            .Returns(Task.CompletedTask);

        await _platformService.CreatePlatformAsync(newPlatformType);

        _unitOfWorkMock.Verify(
                       uow => uow.PlatformRepository.FindAsync(
                       It.IsAny<Expression<Func<PlatformEntity, bool>>>()),
                       Times.Once);

        _unitOfWorkMock.Verify(
                       uow => uow.PlatformRepository.AddAsync(
                       It.Is<PlatformEntity>(p => p.Type == newPlatformType)),
                       Times.Once);

        _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPlatformByIdAsyncShouldThrowNotFoundExceptionWhenPlatformDoesNotExist()
    {
        var nonExistentPlatformId = Guid.NewGuid();

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(nonExistentPlatformId))
            .ReturnsAsync((PlatformEntity?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                   _platformService.GetPlatformByIdAsync(nonExistentPlatformId));

        Assert.Equal($"Platform with id '{nonExistentPlatformId}' does not exist.", exception.Message);

        _unitOfWorkMock.Verify(
                       uow => uow.PlatformRepository.GetByIdAsync(nonExistentPlatformId),
                       Times.Once);
    }

    [Fact]
    public async Task GetPlatformByIdAsyncShouldReturnPlatformWhenItExists()
    {
        var existingPlatformId = Guid.NewGuid();
        var existingPlatformEntity = new PlatformEntity
        {
            Id = existingPlatformId,
            Type = "PlayStation",
        };

        var expectedPlatformModel = new PlatformModel
        {
            Id = existingPlatformId,
            Type = "PlayStation",
        };

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(existingPlatformId))
            .ReturnsAsync(existingPlatformEntity);

        _mapperMock.Setup(m => m.Map<PlatformModel>(existingPlatformEntity))
            .Returns(expectedPlatformModel);

        var result = await _platformService.GetPlatformByIdAsync(existingPlatformId);

        Assert.Equal(expectedPlatformModel.Id, result.Id);
        Assert.Equal(expectedPlatformModel.Type, result.Type);

        _unitOfWorkMock.Verify(
               uow => uow.PlatformRepository.GetByIdAsync(existingPlatformId),
               Times.Once);

        _mapperMock.Verify(
                m => m.Map<PlatformModel>(existingPlatformEntity),
                Times.Once);
    }

    [Fact]
    public async Task GetAllPlatformsAsyncShouldReturnAllPlatforms()
    {
        var platformEntities = new List<PlatformEntity>
    {
        new() { Id = Guid.NewGuid(), Type = "PC" },
        new() { Id = Guid.NewGuid(), Type = "Xbox" },
    };

        var expectedPlatformModels = new List<PlatformModel>
    {
        new() { Id = platformEntities[0].Id, Type = "PC" },
        new() { Id = platformEntities[1].Id, Type = "Xbox" },
    };

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetAllAsync())
            .ReturnsAsync(platformEntities);

        _mapperMock.Setup(m => m.Map<List<PlatformModel>>(platformEntities))
            .Returns(expectedPlatformModels);

        var result = await _platformService.GetAllPlatformsAsync();

        result.Should().BeEquivalentTo(expectedPlatformModels);

        _unitOfWorkMock.Verify(uow => uow.PlatformRepository.GetAllAsync(), Times.Once);
        _mapperMock.Verify(m => m.Map<List<PlatformModel>>(platformEntities), Times.Once);
    }

    [Fact]
    public async Task UpdatePlatformAsyncShouldThrowNotFoundExceptionWhenPlatformDoesNotExist()
    {
        var nonExistentPlatformModel = new PlatformModel
        {
            Id = Guid.NewGuid(),
            Type = "NonExistentPlatform",
        };

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(nonExistentPlatformModel.Id))
            .ReturnsAsync((PlatformEntity?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                          _platformService.UpdatePlatformAsync(nonExistentPlatformModel));

        Assert.Equal($"Platform with id '{nonExistentPlatformModel.Id}' does not exist.", exception.Message);

        _unitOfWorkMock.Verify(
                                  uow => uow.PlatformRepository.GetByIdAsync(nonExistentPlatformModel.Id),
                                  Times.Once);
    }

    [Fact]
    public async Task UpdatePlatformAsyncShouldUpdatePlatformWhenItExists()
    {
        var existingPlatformId = Guid.NewGuid();
        var existingPlatformEntity = new PlatformEntity
        {
            Id = existingPlatformId,
            Type = "OldType",
        };

        var updatedPlatformModel = new PlatformModel
        {
            Id = existingPlatformId,
            Type = "NewType",
        };

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(existingPlatformId))
            .ReturnsAsync(existingPlatformEntity);

        _mapperMock.Setup(m => m.Map(updatedPlatformModel, existingPlatformEntity))
            .Callback<PlatformModel, PlatformEntity>((model, entity) =>
            {
                entity.Type = model.Type;
            });

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.UpdateAsync(existingPlatformEntity))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(uow => uow.SaveAsync())
            .Returns(Task.CompletedTask);

        await _platformService.UpdatePlatformAsync(updatedPlatformModel);

        _unitOfWorkMock.Verify(
               uow => uow.PlatformRepository.GetByIdAsync(existingPlatformId),
               Times.Once);

        _mapperMock.Verify(
                 m => m.Map(updatedPlatformModel, existingPlatformEntity),
                 Times.Once);

        _unitOfWorkMock.Verify(
                 uow => uow.PlatformRepository.UpdateAsync(existingPlatformEntity),
                 Times.Once);

        _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletePlatformAsyncShouldThrowNotFoundExceptionWhenPlatformDoesNotExist()
    {
        var nonExistentPlatformId = Guid.NewGuid();

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(nonExistentPlatformId))
            .ReturnsAsync((PlatformEntity?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                          _platformService.DeletePlatformAsync(nonExistentPlatformId));

        Assert.Equal($"Platform with id '{nonExistentPlatformId}' does not exist.", exception.Message);

        _unitOfWorkMock.Verify(
                  uow => uow.PlatformRepository.GetByIdAsync(nonExistentPlatformId),
                  Times.Once);
    }

    [Fact]
    public async Task DeletePlatformAsyncShouldDeletePlatformWhenItExists()
    {
        var existingPlatformId = Guid.NewGuid();
        var existingPlatformEntity = new PlatformEntity
        {
            Id = existingPlatformId,
            Type = "ToBeDeleted",
        };

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(existingPlatformId))
            .ReturnsAsync(existingPlatformEntity);

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.DeleteAsync(existingPlatformEntity))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(uow => uow.SaveAsync())
            .Returns(Task.CompletedTask);

        await _platformService.DeletePlatformAsync(existingPlatformId);

        _unitOfWorkMock.Verify(
                    uow => uow.PlatformRepository.GetByIdAsync(existingPlatformId),
                    Times.Once);

        _unitOfWorkMock.Verify(
                    uow => uow.PlatformRepository.DeleteAsync(existingPlatformEntity),
                    Times.Once);

        _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGamesByPlatformIdAsyncShouldThrowNotFoundExceptionWhenPlatformDoesNotExist()
    {
        var nonExistentPlatformId = Guid.NewGuid();

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(nonExistentPlatformId))
            .ReturnsAsync((PlatformEntity?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                          _platformService.GetGamesByPlatformIdAsync(nonExistentPlatformId));

        Assert.Equal($"Platform with id '{nonExistentPlatformId}' does not exist.", exception.Message);

        _unitOfWorkMock.Verify(
                  uow => uow.PlatformRepository.GetByIdAsync(nonExistentPlatformId),
                  Times.Once);
    }

    [Fact]
    public async Task GetGamesByPlatformIdAsyncWhenPlatformExistsShouldReturnGames()
    {
        var platformId = Guid.NewGuid();

        var platformEntity = new PlatformEntity
        {
            Id = platformId,
            Type = "PC",
        };

        var gameEntities = new List<GameEntity>
        {
        new() { GameId = Guid.NewGuid(), GameName = "Game 1" },
        new() { GameId = Guid.NewGuid(), GameName = "Game 2" },
        };

        var expectedGameModels = new List<GameModel>
        {
        new() { Id = gameEntities[0].GameId, Name = "Game 1" },
        new() { Id = gameEntities[1].GameId, Name = "Game 2" },
        };

        _unitOfWorkMock
            .Setup(uow => uow.PlatformRepository.GetByIdAsync(platformId))
            .ReturnsAsync(platformEntity);

        _unitOfWorkMock
            .Setup(uow => uow.GamePlatformRepository.GetGamesByPlatformIdAsync(platformId))
            .ReturnsAsync(gameEntities);

        _mapperMock
            .Setup(m => m.Map<List<GameModel>>(gameEntities))
            .Returns(expectedGameModels);

        var result = await _platformService.GetGamesByPlatformIdAsync(platformId);

        result.Should().BeEquivalentTo(expectedGameModels);

        _unitOfWorkMock.Verify(
            uow => uow.PlatformRepository.GetByIdAsync(platformId),
            Times.Once);

        _unitOfWorkMock.Verify(
            uow => uow.GamePlatformRepository.GetGamesByPlatformIdAsync(platformId),
            Times.Once);

        _mapperMock.Verify(
            m => m.Map<List<GameModel>>(gameEntities),
            Times.Once);
    }
}
