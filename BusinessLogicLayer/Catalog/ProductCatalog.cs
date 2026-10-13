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
        var northwindProducts = await ResolveNorthwindAsync();

        return sqlProducts.Concat(northwindProducts).ToList();
    }

    public async Task<ProductEntity?> GetByKeyAsync(string key)
    {
        var sqlProduct = await sqlUnitOfWork.GameRepository.GetQueryable().FirstOrDefaultAsync(p => p.Key == key);

        if (sqlProduct is not null)
        {
            return sqlProduct;
        }

        var northwindProducts = await ResolveNorthwindAsync();

        return northwindProducts.FirstOrDefault(p => p.Key == key);
    }

    public async Task<ProductEntity?> GetByIdAsync(Guid id)
    {
        var sqlProduct = await sqlUnitOfWork.GameRepository.GetFullGame(id);

        if (sqlProduct is not null || !MongoIdConverter.TryGetObjectId(id, out _))
        {
            return sqlProduct;
        }

        var northwindProducts = await ResolveNorthwindAsync();

        return northwindProducts.FirstOrDefault(p => p.Id == id);
    }

    public async Task<ProductEntity> EnsureInGameStoreAsync(string key)
    {
        var sqlProduct = await sqlUnitOfWork.GameRepository.GetQueryable().FirstOrDefaultAsync(p => p.Key == key);

        if (sqlProduct is not null)
        {
            return sqlProduct;
        }

        var northwindProduct = (await ResolveNorthwindAsync()).FirstOrDefault(p => p.Key == key)
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

        var northwindProduct = (await ResolveNorthwindAsync()).FirstOrDefault(p => p.Key == key);

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

    private async Task<List<ProductEntity>> ResolveNorthwindAsync()
    {
        var sqlProducts = await sqlUnitOfWork.GameRepository.GetAllIncludingDeleted().ToListAsync();
        var northwindProducts = (await mongoUnitOfWork.ProductRepository.GetAllAsync()).ToList();

        await AssignMissingGameKeysAsync(northwindProducts, sqlProducts);

        var suppliers = await supplierCatalog.ResolveNorthwindAsync();
        var categories = await categoryCatalog.ResolveNorthwindAsync();

        var options = new NorthwindResolverOptions<ProductEntity, Northwind.ProductEntity>(
            p => NorthwindEntityConverter.ToProduct(p, Lookup(suppliers, p.SupplierID), Lookup(categories, p.CategoryID)),
            p => p.ProductID,
            p => p.Id,
            p => p.Key,
            p => p.IsDeleted);

        // Products without a valid publisher (damaged or deleted) cannot be shown or bought.
        return NorthwindResolver.Resolve(sqlProducts, northwindProducts, options).NorthwindOnly
            .Where(p => p.Supplier is not null)
            .ToList();
    }

    /// <summary>
    /// The game key field in Northwind is managed only by the Game Store: every product gets a unique one.
    /// </summary>
    private async Task AssignMissingGameKeysAsync(List<Northwind.ProductEntity> northwindProducts, List<ProductEntity> sqlProducts)
    {
        var withoutKey = northwindProducts.Where(p => string.IsNullOrWhiteSpace(p.Key)).ToList();

        if (withoutKey.Count == 0)
        {
            return;
        }

        var usedKeys = sqlProducts.Select(p => p.Key)
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
}
