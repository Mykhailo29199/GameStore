using System.Linq.Expressions;
using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

/// <summary>
/// Reads use the IQueryable LINQ provider, updates use FilterDefinition/UpdateDefinition,
/// inserts and deletes are sent as raw database commands. Every write is logged.
/// </summary>
public class MongoDBGenericRepository<T>(
    IMongoDatabase database,
    string collectionName,
    IEntityChangeLogger changeLogger) : IMongoDBGenericRepository<T>
    where T : class
{
    private const string IdField = "_id";

    protected IMongoCollection<T> Collection { get; } = database.GetCollection<T>(collectionName);

    protected string EntityType { get; } = typeof(T) == typeof(BsonDocument) ? collectionName : typeof(T).Name;

    public IQueryable<T> GetAll()
    {
        return Collection.AsQueryable();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Collection.AsQueryable().ToListAsync();
    }

    public async Task<T?> GetByIdAsync(ObjectId id)
    {
        return await Collection.Find(IdFilter(id)).FirstOrDefaultAsync();
    }

    public async Task<bool> CheckIfExistAsync(ObjectId id)
    {
        return await Collection.Find(IdFilter(id)).AnyAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await Collection.AsQueryable().Where(predicate).ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await Collection.AsQueryable().Where(predicate).FirstOrDefaultAsync();
    }

    public async Task<T?> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
    {
        var oldVersion = await Collection.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<T> { ReturnDocument = ReturnDocument.Before });

        if (oldVersion is null)
        {
            return null;
        }

        var oldDocument = oldVersion.ToBsonDocument();
        var newVersion = await Collection.Find(IdFilter(oldDocument[IdField])).FirstOrDefaultAsync();

        await LogAsync(EntityChange.UpdateAction, oldDocument, newVersion?.ToBsonDocument());

        return newVersion;
    }

    public async Task InsertAsync(T entity)
    {
        var document = entity.ToBsonDocument();

        var command = new BsonDocument
        {
            { "insert", collectionName },
            { "documents", new BsonArray { document } },
        };

        await database.RunCommandAsync<BsonDocument>(command);
        await LogAsync(EntityChange.CreateAction, null, document);
    }

    public async Task DeleteAsync(ObjectId id)
    {
        var oldVersion = await GetByIdAsync(id);

        if (oldVersion is null)
        {
            return;
        }

        var command = new BsonDocument
        {
            { "delete", collectionName },
            {
                "deletes", new BsonArray
                {
                    new BsonDocument
                    {
                        { "q", new BsonDocument(IdField, id) },
                        { "limit", 1 },
                    },
                }
            },
        };

        await database.RunCommandAsync<BsonDocument>(command);
        await LogAsync(EntityChange.DeleteAction, oldVersion.ToBsonDocument(), null);
    }

    protected static FilterDefinition<T> IdFilter(BsonValue id)
    {
        return Builders<T>.Filter.Eq(IdField, id);
    }

    private Task LogAsync(string action, BsonDocument? oldVersion, BsonDocument? newVersion)
    {
        return changeLogger.LogAsync(new EntityChange(
            EntityChange.NorthwindDatabase,
            action,
            EntityType,
            oldVersion,
            newVersion));
    }
}
