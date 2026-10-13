using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DataAccessLayer.Mongo.Serializers;

/// <summary>
/// Helpers for reading damaged Northwind values: numbers stored as strings, "NULL" literals,
/// numeric postal codes and other unexpected BSON types are converted instead of throwing.
/// </summary>
internal static class TolerantBsonValue
{
    private const string NullLiteral = "NULL";

    public static BsonValue Read(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var value = BsonValueSerializer.Instance.Deserialize(context, args);

        return value.IsString && string.Equals(value.AsString.Trim(), NullLiteral, StringComparison.OrdinalIgnoreCase)
            ? BsonNull.Value
            : value;
    }

    /// <summary>
    /// Returns null for null, undefined and values that cannot be read as a number.
    /// </summary>
    public static double? ToDouble(BsonValue value)
    {
        return value switch
        {
            { IsNumeric: true } => value.ToDouble(),
            { IsBoolean: true } => value.AsBoolean ? 1 : 0,
            { IsString: true } => ParseDouble(value.AsString),
            _ => null,
        };
    }

    private static double? ParseDouble(string text)
    {
        return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}
