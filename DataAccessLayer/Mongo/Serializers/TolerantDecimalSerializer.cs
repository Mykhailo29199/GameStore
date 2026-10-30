using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DataAccessLayer.Mongo.Serializers;

/// <summary>
/// Reads money stored as any number or numeric string. Writes a double, the type Northwind stores: a stored 19.45 is
/// slightly less than the exact decimal 19.45, so a filter written as Decimal128 would miss it.
/// </summary>
public sealed class TolerantDecimalSerializer : SerializerBase<decimal>
{
    public override decimal Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return TolerantBsonValue.ToDecimal(TolerantBsonValue.Read(context, args)) ?? 0;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, decimal value)
    {
        context.Writer.WriteDouble((double)value);
    }
}
