using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DataAccessLayer.Mongo.Serializers;

public sealed class TolerantNullableInt32Serializer : SerializerBase<int?>
{
    public override int? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var number = TolerantBsonValue.ToDouble(TolerantBsonValue.Read(context, args));

        return number.HasValue ? (int)Math.Round(number.Value) : null;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, int? value)
    {
        if (value.HasValue)
        {
            context.Writer.WriteInt32(value.Value);
        }
        else
        {
            context.Writer.WriteNull();
        }
    }
}
