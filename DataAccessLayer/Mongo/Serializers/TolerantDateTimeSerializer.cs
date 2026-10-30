using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DataAccessLayer.Mongo.Serializers;

public sealed class TolerantDateTimeSerializer : SerializerBase<DateTime>
{
    private readonly TolerantNullableDateTimeSerializer _inner = new();

    public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return _inner.Deserialize(context, args) ?? DateTime.MinValue;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
    {
        _inner.Serialize(context, args, value);
    }
}
