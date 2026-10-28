using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

/// <summary>
/// Writes all logs of one save with a single raw insert command.
/// </summary>
public class EntityChangeLogRepository(IMongoDatabase database) : IEntityChangeLogRepository
{
    private const string CollectionName = "logs";

    public async Task AddRangeAsync(IReadOnlyCollection<EntityChangeLogEntity> logs)
    {
        if (logs.Count > 0)
        {
            await database.RunCommandAsync<BsonDocument>(CreateInsertCommand(logs));
        }
    }

    public void AddRange(IReadOnlyCollection<EntityChangeLogEntity> logs)
    {
        if (logs.Count > 0)
        {
            database.RunCommand<BsonDocument>(CreateInsertCommand(logs));
        }
    }

    private static BsonDocument CreateInsertCommand(IEnumerable<EntityChangeLogEntity> logs)
    {
        var documents = new BsonArray();

        foreach (var log in logs)
        {
            log.Id = ObjectId.GenerateNewId();

            // A missing version is stored as null; the default serializer would write { _csharpnull: true }.
            var document = log.ToBsonDocument();
            document[nameof(log.OldVersion)] = (BsonValue?)log.OldVersion ?? BsonNull.Value;
            document[nameof(log.NewVersion)] = (BsonValue?)log.NewVersion ?? BsonNull.Value;

            documents.Add(document);
        }

        return new BsonDocument
        {
            { "insert", CollectionName },
            { "documents", documents },
        };
    }
}
