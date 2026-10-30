using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class EntityChangeLoggingTests
{
    [Fact]
    public void Update_ShouldKeepOnlyKeyAndChangedFields()
    {
        // Arrange
        var oldVersion = new BsonDocument { { "_id", 1 }, { "ProductName", "Chai" }, { "Views", 5 }, { "Discontinued", true } };
        var newVersion = new BsonDocument { { "_id", 1 }, { "ProductName", "Chai" }, { "Views", 6 }, { "Key", "chai" } };

        // Act
        var change = EntityChange.Update(EntityChange.NorthwindDatabase, "ProductEntity", oldVersion, newVersion, ["_id"]);

        // Assert
        Assert.Equal(EntityChange.UpdateAction, change.Action);
        Assert.Equal(new BsonDocument { { "_id", 1 }, { "Views", 5 }, { "Discontinued", true } }, change.OldVersion);
        Assert.Equal(new BsonDocument { { "_id", 1 }, { "Views", 6 }, { "Key", "chai" } }, change.NewVersion);
    }

    [Fact]
    public async Task LogAsync_ShouldStoreAllChangesWithOneWrite()
    {
        // Arrange
        var repositoryMock = new Mock<IEntityChangeLogRepository>();
        var logger = new EntityChangeLogger(repositoryMock.Object, NullLogger<EntityChangeLogger>.Instance);
        EntityChange[] changes =
        [
            new(EntityChange.GameStoreDatabase, EntityChange.CreateAction, "OrderEntity", null, []),
            new(EntityChange.GameStoreDatabase, EntityChange.UpdateAction, "ProductEntity", [], []),
        ];

        // Act
        await logger.LogAsync(changes);

        // Assert
        repositoryMock.Verify(
            x => x.AddRangeAsync(It.Is<IReadOnlyCollection<EntityChangeLogEntity>>(logs => logs.Count == 2)),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_WhenStoreFails_ShouldNotThrow()
    {
        // Arrange
        var repositoryMock = new Mock<IEntityChangeLogRepository>();
        repositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<IReadOnlyCollection<EntityChangeLogEntity>>()))
            .ThrowsAsync(new InvalidOperationException("Mongo is down"));
        var logger = new EntityChangeLogger(repositoryMock.Object, NullLogger<EntityChangeLogger>.Instance);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            logger.LogAsync([new(EntityChange.GameStoreDatabase, EntityChange.DeleteAction, "CommentEntity", [], null)]));

        // Assert
        Assert.Null(exception);
    }
}
