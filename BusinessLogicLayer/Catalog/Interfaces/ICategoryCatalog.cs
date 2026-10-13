using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Catalog.Interfaces;

/// <summary>
/// Categories (genres) from both databases.
/// </summary>
public interface ICategoryCatalog
{
    Task<List<CategoryEntity>> GetAllAsync();

    Task<CategoryEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Returns the tracked Game Store category, copying it from Northwind first when needed. The caller saves changes.
    /// </summary>
    Task<CategoryEntity> EnsureInGameStoreAsync(Guid id);

    Task<bool> HasNorthwindCounterpartAsync(CategoryEntity category);

    Task<NorthwindResolution<CategoryEntity>> ResolveNorthwindAsync();
}
