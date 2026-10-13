using DataAccessLayer.Entities.CommonEntities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Entities.NorthwindMDBEntities;

[BsonIgnoreExtraElements]
public class EmployeeEntity : EmployeeBaseEntity
{
    [BsonId]
    public ObjectId InternalId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public string? ReportsTo { get; set; }

    public string? Photo { get; set; }
}