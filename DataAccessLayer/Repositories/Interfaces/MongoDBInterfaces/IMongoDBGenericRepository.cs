using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface IMongoDBGenericRepository<T> : ICommonReadGenericRepository<T, ObjectId>
    where T : class
{
    Task<T?> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);

    Task InsertAsync(T entity);

    Task DeleteAsync(ObjectId id);
}
