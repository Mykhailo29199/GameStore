using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IGenreRepository : IGenericRepository<GenreEntity>
{
    Task<GenreEntity?> GetWithSubGenresAsync(Guid id);
}
