using System.Linq.Expressions;
using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using FluentAssertions;
using GameStore.Shared.Exceptions;
using Moq;
using Xunit;
using static GameStoreUnitTests.GameStore.Services.Tests.TestConstants;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class PlatformServiceTests
{
    private readonly PlatformService _platformService;
    private readonly Mock<ISqlServerUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;

    public PlatformServiceTests()
    {
        _unitOfWorkMock = new Mock<ISqlServerUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _platformService = new PlatformService(_unitOfWorkMock.Object, _mapperMock.Object, TestLocalization.CreateLocalizer(_unitOfWorkMock));
    }

    [Fact]
    public async Task CreatePlatformAsync_WhenPlatformTypeExists_ThrowsBadRequestException()
    {
        // Arrange
        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.FindAsync(It.IsAny<Expression<Func<PlatformEntity, bool>>>()))
            .ReturnsAsync(new List<PlatformEntity> { ExistingPlatform1 });

        // Act
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => _platformService.CreatePlatformAsync(ExistingPlatform1.Type));

        exception.Message.Should().Be($"Platform with type '{ExistingPlatform1.Type}' already exists.");

        // Assert
        _unitOfWorkMock.Verify(uow => uow.PlatformRepository.FindAsync(It.IsAny<Expression<Func<PlatformEntity, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task CreatePlatformAsync_WhenPlatformTypeNotExists_AddsPlatform()
    {
        // Arrange
        var newPlatformType = "PlayStation";

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.FindAsync(It.IsAny<Expression<Func<PlatformEntity, bool>>>()))
            .ReturnsAsync(new List<PlatformEntity>());

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

        // Act
        await _platformService.CreatePlatformAsync(newPlatformType);

        // Assert
        _unitOfWorkMock.Verify(uow => uow.PlatformRepository.FindAsync(It.IsAny<Expression<Func<PlatformEntity, bool>>>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.PlatformRepository.AddAsync(It.Is<PlatformEntity>(p => p.Type == newPlatformType)), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPlatformByIdAsync_WhenPlatformDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(NonExistingPlatformId))
            .ReturnsAsync((PlatformEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _platformService.GetPlatformByIdAsync(NonExistingPlatformId));

        // Assert
        exception.Message.Should().Be($"Platform with id '{NonExistingPlatformId}' does not exist.");
    }

    [Fact]
    public async Task GetPlatformByIdAsync_WhenPlatformExists_ShouldReturnMappedPlatform()
    {
        // Arrange
        var expectedModel = new PlatformModel
        {
            Id = ExistingPlatform1.Id,
            Type = ExistingPlatform1.Type,
        };

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(ExistingPlatformId))
            .ReturnsAsync(ExistingPlatform1);

        _mapperMock.Setup(m => m.Map<PlatformModel>(ExistingPlatform1))
            .Returns(expectedModel);

        // Act
        var result = await _platformService.GetPlatformByIdAsync(ExistingPlatform1.Id);

        // Assert
        result.Should().BeEquivalentTo(expectedModel);
    }

    [Fact]
    public async Task GetAllPlatformsAsync_ShouldReturnAllMappedPlatforms()
    {
        // Arrange
        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetAllAsync())
            .ReturnsAsync(ExistingPlatforms);

        var expectedModels = ExistingPlatforms
            .Select(p => new PlatformModel { Id = p.Id, Type = p.Type }).ToList();

        _mapperMock.Setup(m => m.Map<List<PlatformModel>>(ExistingPlatforms))
            .Returns(expectedModels);

        // Act
        var result = await _platformService.GetAllPlatformsAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedModels);
    }

    [Fact]
    public async Task UpdatePlatformAsync_WhenPlatformDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var platformModel = new PlatformModel
        {
            Id = NonExistingPlatformId,
            Type = "NonExisting",
        };

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(platformModel.Id))
            .ReturnsAsync((PlatformEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _platformService.UpdatePlatformAsync(platformModel));

        // Assert
        Assert.Equal($"Platform with id '{platformModel.Id}' does not exist.", exception.Message);
    }

    [Fact]
    public async Task UpdatePlatformAsync_WhenPlatformExists_ShouldUpdateAndSave()
    {
        // Arrange
        var platformModel = new PlatformModel
        {
            Id = ExistingPlatform1.Id,
            Type = "UpdatedType",
        };

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(ExistingPlatformId))
            .ReturnsAsync(ExistingPlatform1);

        _unitOfWorkMock.Setup(uow => uow.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _platformService.UpdatePlatformAsync(platformModel);

        // Assert
        ExistingPlatform1.Type.Should().Be("UpdatedType");
        _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletePlatformAsync_WhenPlatformDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(NonExistingPlatformId))
            .ReturnsAsync((PlatformEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _platformService.DeletePlatformAsync(NonExistingPlatformId));

        // Assert
        Assert.Equal($"Platform with id '{NonExistingPlatformId}' does not exist.", exception.Message);
    }

    [Fact]
    public async Task DeletePlatformAsync_WhenPlatformExists_ShouldDeleteAndSave()
    {
        // Arrange
        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(ExistingPlatformId))
            .ReturnsAsync(ExistingPlatform1);

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.DeleteAsync(ExistingPlatform1))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(uow => uow.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _platformService.DeletePlatformAsync(ExistingPlatform1.Id);

        // Assert
        _unitOfWorkMock.Verify(uow => uow.PlatformRepository.DeleteAsync(ExistingPlatform1), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGamesByPlatformIdAsync_WhenPlatformDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(NonExistingPlatformId))
            .ReturnsAsync((PlatformEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _platformService.GetGamesByPlatformIdAsync(NonExistingPlatformId));

        // Assert
        Assert.Equal($"Platform with id '{NonExistingPlatformId}' does not exist.", exception.Message);
    }

    [Fact]
    public async Task GetGamesByPlatformIdAsync_WhenPlatformExists_ShouldReturnMappedGames()
    {
        // Arrange
        var gameEntities = new List<ProductEntity>
    {
        new() { Id = ExistingGameId, ProductName = "Game 1", Key = ExistingGameKey },
        new() { Id = ExistingGameId2, ProductName = "Game 2", Key = ExistingGameKey2 },
    };

        var gameModels = gameEntities.Select(g => new GameModel
        {
            Id = g.Id,
            Name = g.ProductName,
            Key = g.Key,
        }).ToList();

        _unitOfWorkMock.Setup(uow => uow.PlatformRepository.GetByIdAsync(ExistingPlatformId))
            .ReturnsAsync(ExistingPlatform1);

        _unitOfWorkMock.Setup(uow => uow.ProductPlatformRepository.GetGamesByPlatformIdAsync(ExistingPlatform1.Id))
            .ReturnsAsync(gameEntities);

        _mapperMock.Setup(m => m.Map<List<GameModel>>(gameEntities))
            .Returns(gameModels);

        // Act
        var result = await _platformService.GetGamesByPlatformIdAsync(ExistingPlatform1.Id);

        // Assert
        result.Should().BeEquivalentTo(gameModels);
    }
}
