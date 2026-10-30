using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using MongoDB.Bson;

namespace BusinessLogicLayer.Services;

/// <summary>
/// Shippers are stored only in Northwind and have a dynamic structure, so every document field is returned as is.
/// </summary>
public class ShipperService(IMongoDBUnitOfWork mongoUnitOfWork) : IShipperService
{
    public async Task<IEnumerable<IDictionary<string, object?>>> GetShippersAsync()
    {
        var shippers = await mongoUnitOfWork.ShipperRepository.GetAllAsync();

        return shippers.Select(ToDictionary).ToList();
    }

    private static Dictionary<string, object?> ToDictionary(BsonDocument document)
    {
        return document.Elements.ToDictionary(e => e.Name, e => ToPlainValue(e.Value));
    }

    private static object? ToPlainValue(BsonValue value)
    {
        return value switch
        {
            BsonNull or BsonUndefined => null,
            BsonObjectId objectId => objectId.Value.ToString(),
            BsonDocument document => ToDictionary(document),
            BsonArray array => array.Select(ToPlainValue).ToList(),
            _ => BsonTypeMapper.MapToDotNetValue(value),
        };
    }
}
