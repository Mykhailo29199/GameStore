using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

public class EntityChangeLogRepository(IMongoDatabase database) : IEntityChangeLogRepository
{
    private const string CollectionName = "logs";

    public async Task AddAsync(EntityChangeLogEntity log)
    {
        log.Id = ObjectId.GenerateNewId();

        var command = new BsonDocument
        {
            { "insert", CollectionName },
            { "documents", new BsonArray { log.ToBsonDocument() } },
        };

        await database.RunCommandAsync<BsonDocument>(command);
    }
}
