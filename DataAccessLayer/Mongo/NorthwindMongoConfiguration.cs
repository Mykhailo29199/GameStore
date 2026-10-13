using DataAccessLayer.Entities.CommonEntities;
using DataAccessLayer.Mongo.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace DataAccessLayer.Mongo;

public static class NorthwindMongoConfiguration
{
    private const string NorthwindEntitiesNamespace = "DataAccessLayer.Entities.NorthwindMDBEntities";
    private const string CommonEntitiesNamespace = "DataAccessLayer.Entities.CommonEntities";

    private static int _isConfigured;

    /// <summary>
    /// Registers conventions and class maps for the Northwind collections. Must run before the first Northwind query.
    /// </summary>
    public static void Configure()
    {
        if (Interlocked.Exchange(ref _isConfigured, 1) == 1)
        {
            return;
        }

        var conventions = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true),
            new TolerantPrimitivesConvention(),
        };

        ConventionRegistry.Register(
            "NorthwindTolerantData",
            conventions,
            type => type.Namespace is NorthwindEntitiesNamespace or CommonEntitiesNamespace);

        BsonClassMap.RegisterClassMap<CategoryBaseEntity>(map =>
        {
            map.AutoMap();
            map.MapMember(c => c.Name).SetElementName("CategoryName");
        });
    }

    private sealed class TolerantPrimitivesConvention : ConventionBase, IMemberMapConvention
    {
        // The serializers are stateless, so one instance per member type is shared by all member maps.
        private static readonly Dictionary<Type, IBsonSerializer> Serializers = new()
        {
            [typeof(string)] = new TolerantStringSerializer(),
            [typeof(int)] = new TolerantInt32Serializer(),
            [typeof(int?)] = new TolerantNullableInt32Serializer(),
            [typeof(double)] = new TolerantDoubleSerializer(),
            [typeof(double?)] = new TolerantNullableDoubleSerializer(),
            [typeof(DateTime)] = new TolerantDateTimeSerializer(),
            [typeof(DateTime?)] = new TolerantNullableDateTimeSerializer(),
        };

        public void Apply(BsonMemberMap memberMap)
        {
            if (Serializers.TryGetValue(memberMap.MemberType, out var serializer))
            {
                memberMap.SetSerializer(serializer);
            }
        }
    }
}
