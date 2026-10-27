using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface IMongoDBGenericRepository<T> : ICommonReadGenericRepository<T, ObjectId>
    where T : class
{
    /// <summary>
    /// Returns documents that match all the predicates, filtered in the database.
    /// </summary>
    Task<List<T>> FindAsync(IEnumerable<Expression<Func<T, bool>>> predicates);

    Task<T?> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);

    Task InsertAsync(T entity);

    Task DeleteAsync(ObjectId id);
}
