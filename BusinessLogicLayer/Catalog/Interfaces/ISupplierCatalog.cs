using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Catalog.Interfaces;

/// <summary>
/// Suppliers (publishers) from both databases. Company name is the unique mapping key.
/// </summary>
public interface ISupplierCatalog
{
    Task<List<SupplierEntity>> GetAllAsync();

    Task<SupplierEntity?> GetByIdAsync(Guid id);

    Task<SupplierEntity?> GetByCompanyNameAsync(string companyName);

    /// <summary>
    /// Returns the tracked Game Store supplier, copying it from Northwind first when needed. The caller saves changes.
    /// </summary>
    Task<SupplierEntity> EnsureInGameStoreAsync(Guid id);

    Task<bool> HasNorthwindCounterpartAsync(SupplierEntity supplier);

    Task<NorthwindResolution<SupplierEntity>> ResolveNorthwindAsync();
}
