using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DataAccessLayer.Mongo.Serializers;

public sealed class TolerantNullableDoubleSerializer : SerializerBase<double?>
{
    public override double? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return TolerantBsonValue.ToDouble(TolerantBsonValue.Read(context, args));
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, double? value)
    {
        if (value.HasValue)
        {
            context.Writer.WriteDouble(value.Value);
        }
        else
        {
            context.Writer.WriteNull();
        }
    }
}
