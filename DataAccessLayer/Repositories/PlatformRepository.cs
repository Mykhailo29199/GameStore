using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.Repositories;

public class PlatformRepository(GameStoreContext context) : GenericRepository<PlatformEntity>(context), IPlatformRepository
{
}
