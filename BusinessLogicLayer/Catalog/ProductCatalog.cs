using System.Text.RegularExpressions;
using BusinessLogicLayer.Catalog.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Mongo;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Northwind = DataAccessLayer.Entities.NorthwindMDBEntities;

namespace BusinessLogicLayer.Catalog;

public partial class ProductCatalog(
    ISqlServerUnitOfWork sqlUnitOfWork,
    IMongoDBUnitOfWork mongoUnitOfWork,
    ISupplierCatalog supplierCatalog,
    ICategoryCatalog categoryCatalog) : IProductCatalog
{
    private const string NorthwindKeyPrefix = "nw_";

    public async Task<List<ProductEntity>> GetAllAsync()
    {
        var sqlProducts = await sqlUnitOfWork.GameRepository.GetQueryable().AsNoTracking().ToListAsync();
        var sqlProductsIncludingDeleted = await sqlUnitOfWork.GameRepository.GetAllIncludingDeleted().ToListAsync();
        var northwindProducts = await mongoUnitOfWork.ProductRepository.GetAllAsync();

        var northwindOnly = Resolve(
            sqlProductsIncludingDeleted,
            northwindProducts,
            await supplierCatalog.ResolveNorthwindAsync(),
            await categoryCatalog.ResolveNorthwindAsync());

        return sqlProducts.Concat(northwindOnly).ToList();
    }

    public async Task<ProductEntity?> GetByKeyAsync(string key)
    {
        var sqlProduct = await sqlUnitOfWork.GameRepository.GetQueryable().FirstOrDefaultAsync(p => p.Key == key);

        return sqlProduct ?? await GetNorthwindOnlyAsync(await mongoUnitOfWork.ProductRepository.GetByKeyAsync(key));
    }

    public async Task<ProductEntity?> GetByIdAsync(Guid id)
    {
        var sqlProduct = await sqlUnitOfWork.GameRepository.GetFullGame(id);

        if (sqlProduct is not null || !MongoIdConverter.TryGetObjectId(id, out var objectId))
        {
            return sqlProduct;
        }

        return await GetNorthwindOnlyAsync(await mongoUnitOfWork.ProductRepository.GetByIdAsync(objectId));
    }

    public async Task<ProductEntity> EnsureInGameStoreAsync(string key)
    {
        var sqlProduct = await sqlUnitOfWork.GameRepository.GetQueryable().FirstOrDefaultAsync(p => p.Key == key);

        if (sqlProduct is not null)
        {
            return sqlProduct;
        }

        var northwindProduct = await GetNorthwindOnlyAsync(await mongoUnitOfWork.ProductRepository.GetByKeyAsync(key))
            ?? throw new NotFoundException($"Game with Key '{key}' was not found.");

        await AddGameStoreCopyAsync(northwindProduct, isDeleted: false);
        await sqlUnitOfWork.SaveAsync();

        return await sqlUnitOfWork.GameRepository.GetFullGame(northwindProduct.Id);
    }

    public async Task MarkNorthwindProductDeletedAsync(ProductEntity product)
    {
        await AddGameStoreCopyAsync(product, isDeleted: true);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        var existsInGameStore = await sqlUnitOfWork.GameRepository.GetAllIncludingDeleted().AnyAsync(p => p.Key == key);

        return existsInGameStore || await mongoUnitOfWork.ProductRepository.GetByKeyAsync(key) is not null;
    }

    public async Task<bool> HasNorthwindCounterpartAsync(ProductEntity product)
    {
        // A Northwind product is matched either by the game key or by the id of a copied product.
        return await mongoUnitOfWork.ProductRepository.GetByKeyAsync(product.Key) is not null
            || (MongoIdConverter.TryGetObjectId(product.Id, out var objectId)
                && await mongoUnitOfWork.ProductRepository.CheckIfExistAsync(objectId));
    }

    public async Task IncreaseViewsAsync(string key)
    {
        var sqlProduct = await sqlUnitOfWork.GameRepository.GetGameByKeyAsync(key);

        if (sqlProduct is not null)
        {
            sqlProduct.Views++;
            await sqlUnitOfWork.SaveAsync();
            return;
        }

        var northwindProduct = await GetNorthwindOnlyAsync(await mongoUnitOfWork.ProductRepository.GetByKeyAsync(key));

        if (northwindProduct is not null && MongoIdConverter.TryGetObjectId(northwindProduct.Id, out var objectId))
        {
            await mongoUnitOfWork.ProductRepository.IncrementViewsAsync(objectId);
        }
    }

    public async Task DecreaseUnitsInStockAsync(ProductEntity product, int quantity)
    {
        var northwindProduct = await mongoUnitOfWork.ProductRepository.GetByKeyAsync(product.Key);

        var currentUnits = northwindProduct is null
            ? product.UnitsInStock
            : Math.Min(product.UnitsInStock, northwindProduct.UnitsInStock);

        var unitsInStock = Math.Max(currentUnits - quantity, 0);

        product.UnitsInStock = unitsInStock;

        if (northwindProduct is not null)
        {
            await mongoUnitOfWork.ProductRepository.SetUnitsInStockAsync(northwindProduct.InternalId, unitsInStock);
        }
    }

    /// <summary>
    /// The game key field in Northwind is managed only by the Game Store: every product gets a unique one.
    /// </summary>
    public async Task AssignMissingGameKeysAsync()
    {
        var northwindProducts = (await mongoUnitOfWork.ProductRepository.GetAllAsync()).ToList();
        var withoutKey = northwindProducts.Where(p => string.IsNullOrWhiteSpace(p.Key)).ToList();

        if (withoutKey.Count == 0)
        {
            return;
        }

        var sqlKeys = await sqlUnitOfWork.GameRepository.GetAllIncludingDeleted().Select(p => p.Key).ToListAsync();
        var usedKeys = sqlKeys
            .Concat(northwindProducts.Select(p => p.Key))
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var product in withoutKey)
        {
            product.Key = CreateNorthwindKey(product, usedKeys);
            usedKeys.Add(product.Key);

            await mongoUnitOfWork.ProductRepository.SetGameKeyAsync(product.InternalId, product.Key);
        }
    }

    private static List<ProductEntity> Resolve(
        IEnumerable<ProductEntity> sqlProductsIncludingDeleted,
        IEnumerable<Northwind.ProductEntity> northwindProducts,
        NorthwindResolution<SupplierEntity> suppliers,
        NorthwindResolution<CategoryEntity> categories)
    {
        // Products without a valid publisher (damaged or deleted) cannot be shown or bought, so they take no part in the merge.
        var withPublisher = new List<NorthwindProduct>();

        foreach (var product in northwindProducts)
        {
            if (Lookup(suppliers, product.SupplierID) is { } supplier)
            {
                withPublisher.Add(new NorthwindProduct(product, supplier));
            }
        }

        var options = new NorthwindResolverOptions<ProductEntity, NorthwindProduct>(
            p => NorthwindEntityConverter.ToProduct(p.Document, p.Supplier, Lookup(categories, p.Document.CategoryID)),
            p => p.Document.ProductID,
            p => p.Id,
            p => p.Key,
            p => p.IsDeleted);

        // A product without a game key cannot be opened; keys are assigned when the application starts.
        return NorthwindResolver.Resolve(sqlProductsIncludingDeleted, withPublisher, options).NorthwindOnly
            .Where(p => !string.IsNullOrWhiteSpace(p.Key))
            .ToList();
    }

    private static T? Lookup<T>(NorthwindResolution<T> resolution, int? legacyId)
        where T : class
    {
        return legacyId.HasValue ? resolution.ByLegacyId.GetValueOrDefault(legacyId.Value) : null;
    }

    private static string CreateNorthwindKey(Northwind.ProductEntity product, HashSet<string> usedKeys)
    {
        var name = product.ProductName ?? product.ProductID?.ToString() ?? product.InternalId.ToString();
        var slug = NonKeyCharacters().Replace(name.ToLowerInvariant(), "_").Trim('_');
        var baseKey = NorthwindKeyPrefix + slug;

        var key = baseKey;
        var counter = 1;

        while (usedKeys.Contains(key))
        {
            key = $"{baseKey}_{counter++}";
        }

        return key;
    }

    [GeneratedRegex(@"[^\p{L}0-9]+")]
    private static partial Regex NonKeyCharacters();

    /// <summary>
    /// Resolves one Northwind product, reading only the records that decide whether it is shown.
    /// </summary>
    private async Task<ProductEntity?> GetNorthwindOnlyAsync(Northwind.ProductEntity? document)
    {
        if (document is null)
        {
            return null;
        }

        var (sqlProducts, related) = await NorthwindResolver.LoadRelatedAsync<ProductEntity, Northwind.ProductEntity>(
            [document],
            p => p.InternalId,
            p => p.Key,
            mongoUnitOfWork.ProductRepository.GetByIdsOrKeysAsync,
            FindSqlProductsAsync);

        var supplierIds = related.Select(p => p.SupplierID).OfType<int>().Distinct().ToList();
        var categoryIds = related.Select(p => p.CategoryID).OfType<int>().Distinct().ToList();

        var northwindOnly = Resolve(
            sqlProducts,
            related,
            await supplierCatalog.ResolveNorthwindAsync(supplierIds),
            await categoryCatalog.ResolveNorthwindAsync(categoryIds));

        var id = MongoIdConverter.ToGuid(document.InternalId);

        return northwindOnly.FirstOrDefault(p => p.Id == id);
    }

    private async Task<List<ProductEntity>> FindSqlProductsAsync(List<Guid> ids, List<string> keys)
    {
        // Products are matched by game key. The default SQL Server collation ignores case, as the duplicate check does.
        return await sqlUnitOfWork.GameRepository.GetAllIncludingDeleted()
            .Where(p => ids.Contains(p.Id) || keys.Contains(p.Key.Trim()))
            .ToListAsync();
    }

    private async Task AddGameStoreCopyAsync(ProductEntity product, bool isDeleted)
    {
        var supplier = await supplierCatalog.EnsureInGameStoreAsync(product.SupplierID);

        var copy = new ProductEntity
        {
            Id = product.Id,
            ProductID = product.ProductID,
            ProductName = product.ProductName,
            Key = product.Key,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            UnitsInStock = product.UnitsInStock,
            UnitsOnOrder = product.UnitsOnOrder,
            ReorderLevel = product.ReorderLevel,
            QuantityPerUnit = product.QuantityPerUnit,
            Discontinued = product.Discontinued,
            Discount = product.Discount,
            Views = product.Views,
            CreatedAt = product.CreatedAt,
            SupplierID = supplier.Id,
            IsDeleted = isDeleted,
        };

        foreach (var productCategory in product.ProductCategories)
        {
            var category = await categoryCatalog.EnsureInGameStoreAsync(productCategory.CategoryId);

            copy.ProductCategories.Add(new ProductCategoryEntity
            {
                ProductId = copy.Id,
                CategoryId = category.Id,
            });
        }

        await sqlUnitOfWork.GameRepository.AddAsync(copy);
    }

    /// <summary>
    /// A Northwind product together with its resolved publisher.
    /// </summary>
    private sealed record NorthwindProduct(Northwind.ProductEntity Document, SupplierEntity Supplier);
}
