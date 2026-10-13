using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface ICustomerRepository : ISqlServerGenericRepository<CustomerEntity, Guid>
{
}
