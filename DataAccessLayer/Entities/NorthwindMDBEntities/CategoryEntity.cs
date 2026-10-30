using DataAccessLayer.Entities.CommonEntities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Entities.NorthwindMDBEntities;

public class CategoryEntity : CategoryBaseEntity
{
    [BsonId]
    public ObjectId InternalId { get; set; }
}
