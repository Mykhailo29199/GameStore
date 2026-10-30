using MongoDB.Bson;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

/// <summary>
/// Shippers have a dynamic structure, so they are read as raw BSON documents.
/// </summary>
public interface IShipperMongoDBRepository : IMongoDBGenericRepository<BsonDocument>
{
}
