using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Catalog.Interfaces;

/// <summary>
/// Products (games) from both databases. Game key is the unique mapping key.
/// </summary>
public interface IProductCatalog
{
    Task<List<ProductEntity>> GetAllAsync();

    Task<ProductEntity?> GetByKeyAsync(string key);

    Task<ProductEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Returns the tracked Game Store product, copying it (with its publisher and genres) from Northwind first when needed.
    /// Northwind data itself is never changed.
    /// </summary>
    Task<ProductEntity> EnsureInGameStoreAsync(string key);

    /// <summary>
    /// Hides a Northwind-only product by saving a deleted copy of it into the Game Store database. The caller saves changes.
    /// </summary>
    Task MarkNorthwindProductDeletedAsync(ProductEntity product);

    Task<bool> KeyExistsAsync(string key);

    Task<bool> HasNorthwindCounterpartAsync(ProductEntity product);

    Task IncreaseViewsAsync(string key);

    /// <summary>
    /// Recalculates units in stock after a payment and synchronizes the value in both databases. The caller saves changes.
    /// </summary>
    Task DecreaseUnitsInStockAsync(ProductEntity product, int quantity);
}
