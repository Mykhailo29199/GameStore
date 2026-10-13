namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface ISqlServerGenericRepository<T, TId> : ICommonReadGenericRepository<T, TId>
    where T : class
    where TId : notnull
{
    Task AddAsync(T entity);

    Task AddRangeAsync(IEnumerable<T> entities);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);

    IQueryable<T> GetAllIncludingDeleted();
}