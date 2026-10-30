using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DataAccessLayer.Mongo.Serializers;

public sealed class TolerantInt32Serializer : SerializerBase<int>
{
    private readonly TolerantNullableInt32Serializer _inner = new();

    public override int Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return _inner.Deserialize(context, args) ?? 0;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, int value)
    {
        context.Writer.WriteInt32(value);
    }
}
