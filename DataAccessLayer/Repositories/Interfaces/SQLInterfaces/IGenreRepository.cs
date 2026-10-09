using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IGenreRepository : IGenericRepository<GenreEntity>
{
    Task<GenreEntity?> GetWithSubGenresAsync(Guid id);
}
