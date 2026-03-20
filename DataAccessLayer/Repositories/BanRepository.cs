using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.Repositories;

public class BanRepository(GameStoreContext context) : GenericRepository<BanEntity>(context), IBanRepository
{
}
