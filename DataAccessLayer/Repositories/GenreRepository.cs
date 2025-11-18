using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.Repositories;

public class GenreRepository(GameStoreContext context) : GenericRepository<GenreEntity>(context), IGenreRepository
{
}
