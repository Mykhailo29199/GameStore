using System.Linq.Expressions;

namespace DataAccessLayer.Repositories.Interfaces;

public interface ICommonReadGenericRepository<T, TId>
    where T : class
    where TId : notnull
{
    Task<IEnumerable<T>> GetAllAsync();

    Task<T?> GetByIdAsync(TId id);

    Task<bool> CheckIfExistAsync(TId id);

    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    IQueryable<T> GetAll();
}
