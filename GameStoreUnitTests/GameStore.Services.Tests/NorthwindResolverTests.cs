using BusinessLogicLayer.Catalog;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Mongo;
using MongoDB.Bson;
using Xunit;
using Northwind = DataAccessLayer.Entities.NorthwindMDBEntities;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class NorthwindResolverTests
{
    private static readonly NorthwindResolverOptions<SupplierEntity, Northwind.SupplierEntity> Options = new(
        NorthwindEntityConverter.ToSupplier,
        s => s.SupplierID,
        s => s.Id,
        s => s.CompanyName,
        s => s.IsDeleted);

    [Fact]
    public void Resolve_WhenGameStoreHasSameName_ShouldPreferGameStoreEntity()
    {
        // Arrange
        var gameStoreSupplier = new SupplierEntity { Id = Guid.NewGuid(), CompanyName = "Exotic Liquids" };
        var northwindSupplier = CreateNorthwindSupplier(1, " exotic liquids ");

        // Act
        var result = NorthwindResolver.Resolve([gameStoreSupplier], [northwindSupplier], Options);

        // Assert
        Assert.Empty(result.NorthwindOnly);
        Assert.Same(gameStoreSupplier, result.ByLegacyId[1]);
    }

    [Fact]
    public void Resolve_WhenNorthwindEntityWasCopiedAndRenamed_ShouldPreferGameStoreCopy()
    {
        // Arrange
        var northwindSupplier = CreateNorthwindSupplier(1, "Exotic Liquids");
        var renamedCopy = new SupplierEntity
        {
            Id = MongoIdConverter.ToGuid(northwindSupplier.InternalId),
            CompanyName = "Exotic Liquids Ltd",
        };

        // Act
        var result = NorthwindResolver.Resolve([renamedCopy], [northwindSupplier], Options);

        // Assert
        Assert.Empty(result.NorthwindOnly);
        Assert.Same(renamedCopy, result.ByLegacyId[1]);
    }

    [Fact]
    public void Resolve_WhenNorthwindHasDuplicates_ShouldUseFirstFound()
    {
        // Arrange
        var first = CreateNorthwindSupplier(1, "Tokyo Traders");
        var second = CreateNorthwindSupplier(2, "Tokyo Traders");

        // Act
        var result = NorthwindResolver.Resolve([], [first, second], Options);

        // Assert
        var supplier = Assert.Single(result.NorthwindOnly);
        Assert.Equal(MongoIdConverter.ToGuid(first.InternalId), supplier.Id);
        Assert.Same(supplier, result.ByLegacyId[2]);
    }

    [Fact]
    public void Resolve_WhenGameStoreEntityIsDeleted_ShouldHideNorthwindEntity()
    {
        // Arrange
        var northwindSupplier = CreateNorthwindSupplier(1, "Exotic Liquids");
        var deletedCopy = new SupplierEntity
        {
            Id = MongoIdConverter.ToGuid(northwindSupplier.InternalId),
            CompanyName = "Exotic Liquids",
            IsDeleted = true,
        };

        // Act
        var result = NorthwindResolver.Resolve([deletedCopy], [northwindSupplier], Options);

        // Assert
        Assert.Empty(result.NorthwindOnly);
        Assert.False(result.ByLegacyId.ContainsKey(1));
    }

    [Fact]
    public void Resolve_WhenNameIsMissing_ShouldSkipDamagedDocument()
    {
        // Arrange
        var damaged = CreateNorthwindSupplier(1, null);

        // Act
        var result = NorthwindResolver.Resolve([], [damaged], Options);

        // Assert
        Assert.Empty(result.NorthwindOnly);
        Assert.Empty(result.ByLegacyId);
    }

    [Fact]
    public void MongoIdConverter_ShouldConvertObjectIdToGuidAndBack()
    {
        // Arrange
        var objectId = ObjectId.GenerateNewId();

        // Act
        var guid = MongoIdConverter.ToGuid(objectId);
        var isNorthwindId = MongoIdConverter.TryGetObjectId(guid, out var decoded);

        // Assert
        Assert.True(isNorthwindId);
        Assert.Equal(objectId, decoded);
    }

    [Fact]
    public void MongoIdConverter_WhenGuidIsNotNorthwindId_ShouldReturnFalse()
    {
        // Act
        var isNorthwindId = MongoIdConverter.TryGetObjectId(Guid.Parse("11112222-3333-4444-5555-666677778888"), out _);

        // Assert
        Assert.False(isNorthwindId);
    }

    private static Northwind.SupplierEntity CreateNorthwindSupplier(int supplierId, string? companyName)
    {
        return new Northwind.SupplierEntity
        {
            InternalId = ObjectId.GenerateNewId(),
            SupplierID = supplierId,
            CompanyName = companyName!,
        };
    }
}
