using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Entities.NorthwindMDBEntities;

public class EntityChangeLogEntity
{
    [BsonId]
    public ObjectId Id { get; set; }

    public DateTime Date { get; set; }

    public string Action { get; set; }

    public string EntityType { get; set; }

    public string Database { get; set; }

    public BsonDocument? OldVersion { get; set; }

    public BsonDocument? NewVersion { get; set; }
}
