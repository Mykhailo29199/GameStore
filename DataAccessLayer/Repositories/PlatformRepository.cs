using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.Repositories;

public class PlatformRepository(GameStoreContext context) : GenericRepository<PlatformEntity>(context), IPlatformRepository
{
}
