using DataAccessLayer.Entities.CommonEntities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccessLayer.Entities.NorthwindMDBEntities;

public class ProductEntity : ProductBaseEntity
{
    [BsonId]
    public ObjectId InternalId { get; set; }

    public int? SupplierID { get; set; }

    public int? CategoryID { get; set; }

    public int? Views { get; set; }
}
