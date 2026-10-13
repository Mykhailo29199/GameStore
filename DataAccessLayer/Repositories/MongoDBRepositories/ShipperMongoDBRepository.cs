using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

public class ShipperMongoDBRepository(IMongoDatabase database, IEntityChangeLogger changeLogger)
    : MongoDBGenericRepository<BsonDocument>(database, "shippers", changeLogger), IShipperMongoDBRepository
{
}
