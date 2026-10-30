using DataAccessLayer.Entities.CommonEntities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Entities.NorthwindMDBEntities;

[BsonIgnoreExtraElements]
public class OrderMongoDBEntity : OrderBaseEntity
{
    [BsonId]
    public ObjectId InternalId { get; set; }

    public int? OrderID { get; set; }

    public int? EmployeeID { get; set; }

    public string? CustomerID { get; set; }

    public string? ShipAddress { get; set; }

    public string? ShipCity { get; set; }
}
