using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class GenreRepository(GameStoreContext context) : GenericRepository<GenreEntity>(context), IGenreRepository
{
    private readonly DbSet<GenreEntity> _dbSet = context.Set<GenreEntity>();

    public async Task<GenreEntity?> GetWithSubGenresAsync(Guid id)
    {
        return await _dbSet
            .Include(g => g.SubGenres) // Завантажуємо дочірні елементи в Change Tracker
            .FirstOrDefaultAsync(g => g.Id == id);
    }
}
