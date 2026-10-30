using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DataAccessLayer.Mongo.Serializers;

public sealed class TolerantNullableDateTimeSerializer : SerializerBase<DateTime?>
{
    public override DateTime? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var value = TolerantBsonValue.Read(context, args);

        return value.IsValidDateTime
            ? value.ToUniversalTime()
            : ParseDateTime(value);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime? value)
    {
        if (value.HasValue)
        {
            var utcValue = value.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value.Value.ToUniversalTime();

            context.Writer.WriteDateTime(BsonUtils.ToMillisecondsSinceEpoch(utcValue));
        }
        else
        {
            context.Writer.WriteNull();
        }
    }

    private static DateTime? ParseDateTime(BsonValue value)
    {
        return value.IsString && DateTime.TryParse(
            value.AsString,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var parsed)
            ? parsed
            : null;
    }
}
