using System.Linq.Expressions;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class GenericSqlRepository<T, TId>(DbContext context) : ISqlServerGenericRepository<T, TId>
    where T : class
    where TId : notnull
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public async Task<bool> CheckIfExistAsync(TId id)
    {
        return await GetByIdAsync(id) != null;
    }

    public async Task DeleteAsync(T entity)
    {
        await Task.Run(() => _dbSet.Remove(entity));
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(TId id)
    {
        return await _dbSet.FindAsync([id]);
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public IQueryable<T> GetAll()
    {
        return _dbSet.AsNoTracking();
    }

    public IQueryable<T> GetAllIncludingDeleted()
    {
        return _dbSet.IgnoreQueryFilters().AsNoTracking();
    }
}
