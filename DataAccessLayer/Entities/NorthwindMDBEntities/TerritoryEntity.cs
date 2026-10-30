using DataAccessLayer.Entities.CommonEntities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Entities.NorthwindMDBEntities;

[BsonIgnoreExtraElements]
public class TerritoryEntity : TerritoryBaseEntity
{
    [BsonId]
    public ObjectId? InternalId { get; set; }
}