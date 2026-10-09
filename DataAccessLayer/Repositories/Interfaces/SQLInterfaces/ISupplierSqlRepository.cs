using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface ISupplierSqlRepository : ISqlServerGenericRepository<SupplierEntity>
{
    Task<IList<ProductEntity>> GetProductsByPublisherNameAsync(string publisherName);

    Task<ProductEntity> GetProductsByPublisherIdAsync(Guid id);
}
