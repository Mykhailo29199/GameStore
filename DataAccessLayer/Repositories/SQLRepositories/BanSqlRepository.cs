using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class BanSqlRepository(GameStoreContext context) : GenericSqlRepository<BanEntity>(context), IBanSqlRepository
{
}
