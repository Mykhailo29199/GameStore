using System.Linq.Expressions;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IGenericRepository<T>
    where T : class
{
    Task<IEnumerable<T>> GetAllAsync();

    Task<T?> GetByIdAsync(Guid id);

    Task AddAsync(T entity);

    Task AddRangeAsync(IEnumerable<T> entities);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);

    Task<bool> CheckIfExistAsync(Guid id);

    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
}