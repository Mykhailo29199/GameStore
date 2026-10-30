using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class PlatformSqlRepository(GameStoreContext context) : GenericSqlRepository<PlatformEntity, Guid>(context), IPlatformSqlRepository
{
}
