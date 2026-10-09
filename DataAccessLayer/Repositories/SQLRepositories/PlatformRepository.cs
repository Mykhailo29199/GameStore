using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class PlatformRepository(GameStoreContext context) : GenericRepository<PlatformEntity>(context), IPlatformRepository
{
}
