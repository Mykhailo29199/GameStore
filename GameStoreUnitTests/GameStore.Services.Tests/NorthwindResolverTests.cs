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

    private static readonly List<Northwind.SupplierEntity> NorthwindSuppliers =
    [
        CreateNorthwindSupplier(1, "Exotic Liquids"),
        CreateNorthwindSupplier(2, " exotic liquids "),
        CreateNorthwindSupplier(3, "Tokyo Traders"),
        CreateNorthwindSupplier(4, "Old Name"),
        CreateNorthwindSupplier(5, "Grandma Kelly's Homestead"),
        CreateNorthwindSupplier(6, "Pavlova, Ltd."),
    ];

    // A renamed copy of supplier 4, a deleted supplier with the name of supplier 5 and a supplier with the name of supplier 6.
    private static readonly List<SupplierEntity> GameStoreSuppliers =
    [
        new() { Id = MongoIdConverter.ToGuid(NorthwindSuppliers[3].InternalId), CompanyName = "New Name" },
        new() { Id = Guid.NewGuid(), CompanyName = "GRANDMA KELLY'S HOMESTEAD", IsDeleted = true },
        new() { Id = Guid.NewGuid(), CompanyName = "Pavlova, Ltd." },
    ];

    public static TheoryData<int> LegacyIds => [1, 2, 3, 4, 5, 6];

    [Theory]
    [MemberData(nameof(LegacyIds))]
    public async Task LoadRelatedAsync_ShouldResolveDocumentAsWholeCollections(int legacyId)
    {
        // Arrange
        var document = NorthwindSuppliers.Single(d => d.SupplierID == legacyId);
        var full = NorthwindResolver.Resolve(GameStoreSuppliers, NorthwindSuppliers, Options);

        // Act
        var (sqlSuppliers, related) = await LoadRelatedSuppliersAsync([document]);
        var partial = NorthwindResolver.Resolve(sqlSuppliers, related, Options);

        // Assert
        var id = MongoIdConverter.ToGuid(document.InternalId);
        Assert.Equal(full.NorthwindOnly.Any(s => s.Id == id), partial.NorthwindOnly.Any(s => s.Id == id));
        Assert.Equal(full.ByLegacyId.GetValueOrDefault(legacyId)?.Id, partial.ByLegacyId.GetValueOrDefault(legacyId)?.Id);
    }

    [Fact]
    public async Task LoadRelatedAsync_ShouldReadOnlyRecordsWithTheSameIdOrName()
    {
        // Act
        var (sqlSuppliers, related) = await LoadRelatedSuppliersAsync([NorthwindSuppliers[0]]);

        // Assert
        Assert.Equal([1, 2], related.Select(d => d.SupplierID));
        Assert.Empty(sqlSuppliers);
    }

    [Fact]
    public async Task LoadRelatedAsync_WhenNoDocumentsAreGiven_ShouldReadNothing()
    {
        // Act
        var (sqlSuppliers, related) = await NorthwindResolver.LoadRelatedAsync<SupplierEntity, Northwind.SupplierEntity>(
            [],
            s => s.InternalId,
            s => s.CompanyName,
            (_, _) => throw new InvalidOperationException("Northwind must not be read."),
            (_, _) => throw new InvalidOperationException("Game Store must not be read."));

        // Assert
        Assert.Empty(related);
        Assert.Empty(sqlSuppliers);
    }

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

    // The fakes compare names the same way as the repositories: ignoring case and surrounding spaces.
    private static Task<(List<SupplierEntity> SqlEntities, List<Northwind.SupplierEntity> Documents)> LoadRelatedSuppliersAsync(
        List<Northwind.SupplierEntity> documents)
    {
        return NorthwindResolver.LoadRelatedAsync(
            documents,
            s => s.InternalId,
            s => s.CompanyName,
            (ids, names) => Task.FromResult(NorthwindSuppliers
                .Where(d => ids.Contains(d.InternalId) || HasName(names, d.CompanyName))
                .ToList()),
            (ids, names) => Task.FromResult(GameStoreSuppliers
                .Where(s => ids.Contains(s.Id) || HasName(names, s.CompanyName))
                .ToList()));
    }

    private static bool HasName(List<string> names, string? name)
    {
        return names.Exists(n => NorthwindResolver.Normalize(n) == NorthwindResolver.Normalize(name));
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
