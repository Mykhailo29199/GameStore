using DataAccessLayer.Entities.CommonEntities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Entities.NorthwindMDBEntities;

public class OrderDetailsEntity : OrderDetailsBaseEntity
{
    [BsonId]
    public ObjectId InternalId { get; set; }

    public int? OrderID { get; set; }

    public int? ProductID { get; set; }
}
