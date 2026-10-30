using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DataAccessLayer.Mongo.Serializers;

public sealed class TolerantNullableDecimalSerializer : SerializerBase<decimal?>
{
    public override decimal? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return TolerantBsonValue.ToDecimal(TolerantBsonValue.Read(context, args));
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, decimal? value)
    {
        if (value.HasValue)
        {
            context.Writer.WriteDouble((double)value.Value);
        }
        else
        {
            context.Writer.WriteNull();
        }
    }
}
