using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DataAccessLayer.Mongo.Serializers;

public sealed class TolerantDoubleSerializer : SerializerBase<double>
{
    public override double Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return TolerantBsonValue.ToDouble(TolerantBsonValue.Read(context, args)) ?? 0;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, double value)
    {
        context.Writer.WriteDouble(value);
    }
}
