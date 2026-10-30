using System.Globalization;
using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

public class OrderMongoDBRepository(IMongoDatabase database, IEntityChangeLogger changeLogger)
    : MongoDBGenericRepository<OrderMongoDBEntity>(database, "orders", changeLogger), IOrderMongoDBRepository
{
    // Northwind dates are imported from CSV as text ("1996-07-04 00:00:00.000").
    // In this format text order matches date order, so the range is compared as text.
    private const string ImportedDateFormat = "yyyy-MM-dd HH:mm:ss.fff";

    public async Task<List<OrderMongoDBEntity>> GetByDateRangeAsync(DateTime? from, DateTime? to)
    {
        if (!from.HasValue && !to.HasValue)
        {
            return await Collection.Find(FilterDefinition<OrderMongoDBEntity>.Empty).ToListAsync();
        }

        // A missing bound is replaced with the widest date, so text that is not a date (for example "NULL") never matches.
        var dateRange = new BsonDocument
        {
            { "$gte", ToImportedDate(from ?? DateTime.MinValue) },
            { "$lt", ToImportedDate(to ?? DateTime.MaxValue) },
        };

        return await Collection.Find(new BsonDocument(nameof(OrderMongoDBEntity.OrderDate), dateRange)).ToListAsync();
    }

    private static string ToImportedDate(DateTime value)
    {
        return value.ToString(ImportedDateFormat, CultureInfo.InvariantCulture);
    }
}
