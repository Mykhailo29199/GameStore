using BusinessLogicLayer.Models.GameModels;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Catalog.Interfaces;

/// <summary>
/// Products (games) from both databases. Game key is the unique mapping key.
/// </summary>
public interface IProductCatalog
{
    Task<List<ProductEntity>> GetAllAsync(bool includeDeleted = false);

    /// <summary>
    /// Returns the Northwind-only products that match the filter. Filtering runs in Northwind; sorting and paging are left
    /// to the caller, which merges these products with the Game Store ones.
    /// </summary>
    Task<List<ProductEntity>> GetNorthwindOnlyAsync(GameFilterModel filter);

    Task<ProductEntity?> GetByKeyAsync(string key, bool includeDeleted = false);

    Task<ProductEntity?> GetByIdAsync(Guid id, bool includeDeleted = false);

    /// <summary>
    /// Returns the tracked Game Store product, copying it (with its publisher and genres) from Northwind first when needed.
    /// Northwind data itself is never changed. The caller saves changes.
    /// </summary>
    Task<ProductEntity> EnsureInGameStoreAsync(string key, bool includeDeleted = false);

    /// <summary>
    /// Hides a Northwind-only product by saving a deleted copy of it into the Game Store database. The caller saves changes.
    /// </summary>
    Task MarkNorthwindProductDeletedAsync(ProductEntity product);

    Task<bool> KeyExistsAsync(string key);

    Task<bool> HasNorthwindCounterpartAsync(ProductEntity product);

    /// <summary>
    /// Counts a view of the game. A Game Store game is changed in the unit of work, so the caller saves changes.
    /// </summary>
    Task IncreaseViewsAsync(string key);

    /// <summary>
    /// Copies the saved units in stock of Game Store products to their Northwind products. The Game Store value is the
    /// source of truth, so it is called after the Game Store changes are saved, and a failed call is fixed by the next one.
    /// </summary>
    Task SyncUnitsInStockAsync(IReadOnlyCollection<ProductEntity> products);

    /// <summary>
    /// Gives a unique game key to every Northwind product that has none. Runs once when the application starts,
    /// so reading the catalog never writes into Northwind.
    /// </summary>
    Task AssignMissingGameKeysAsync();
}
