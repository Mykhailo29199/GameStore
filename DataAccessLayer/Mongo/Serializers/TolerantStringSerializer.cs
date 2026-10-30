using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DataAccessLayer.Mongo.Serializers;

public sealed class TolerantStringSerializer : SerializerBase<string?>
{
    public override string? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var value = TolerantBsonValue.Read(context, args);

        return value switch
        {
            { IsBsonNull: true } or { IsBsonUndefined: true } => null,
            { IsString: true } => value.AsString,
            _ => value.ToString(),
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string? value)
    {
        if (value is null)
        {
            context.Writer.WriteNull();
        }
        else
        {
            context.Writer.WriteString(value);
        }
    }
}
