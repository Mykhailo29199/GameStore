using DataAccessLayer.Mongo.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Xunit;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class TolerantSerializersTests
{
    [Fact]
    public void StringSerializer_ShouldReadValuesOfAnyType()
    {
        var serializer = new TolerantStringSerializer();

        Assert.Equal("EC1 4SD", Deserialize(serializer, new BsonString("EC1 4SD")));
        Assert.Equal("70117", Deserialize(serializer, new BsonInt32(70117)));
        Assert.Null(Deserialize(serializer, BsonNull.Value));
    }

    [Theory]
    [InlineData("NULL")]
    [InlineData("null")]
    [InlineData(" NULL ")]
    public void StringSerializer_ShouldTreatNullLiteralAsNull(string literal)
    {
        Assert.Null(Deserialize(new TolerantStringSerializer(), new BsonString(literal)));
    }

    [Fact]
    public void NullableInt32Serializer_ShouldReadNumbersStoredWithAnotherType()
    {
        var serializer = new TolerantNullableInt32Serializer();

        Assert.Equal(42, Deserialize(serializer, new BsonInt32(42)));
        Assert.Equal(42, Deserialize(serializer, new BsonString("42")));
        Assert.Equal(3, Deserialize(serializer, new BsonDouble(3.0)));
        Assert.Equal(7, Deserialize(serializer, new BsonInt64(7)));
        Assert.Equal(1, Deserialize(serializer, BsonBoolean.True));
    }

    [Fact]
    public void NullableInt32Serializer_WhenValueIsNotANumber_ShouldReturnNull()
    {
        var serializer = new TolerantNullableInt32Serializer();

        Assert.Null(Deserialize(serializer, new BsonString("ten boxes")));
        Assert.Null(Deserialize(serializer, new BsonString("NULL")));
        Assert.Null(Deserialize(serializer, BsonNull.Value));
    }

    [Fact]
    public void Int32Serializer_WhenValueIsMissingOrDamaged_ShouldReturnZero()
    {
        var serializer = new TolerantInt32Serializer();

        Assert.Equal(0, Deserialize(serializer, BsonNull.Value));
        Assert.Equal(0, Deserialize(serializer, new BsonString("damaged")));
        Assert.Equal(39, Deserialize(serializer, new BsonString("39")));
    }

    [Fact]
    public void DoubleSerializers_ShouldReadNumbersStoredAsStrings()
    {
        Assert.Equal(18.5, Deserialize(new TolerantNullableDoubleSerializer(), new BsonString("18.5")));
        Assert.Equal(18, Deserialize(new TolerantNullableDoubleSerializer(), new BsonInt32(18)));
        Assert.Null(Deserialize(new TolerantNullableDoubleSerializer(), new BsonString("free")));
        Assert.Equal(0, Deserialize(new TolerantDoubleSerializer(), BsonNull.Value));
    }

    [Fact]
    public void NullableDateTimeSerializer_ShouldReadDatesAndDateStrings()
    {
        var serializer = new TolerantNullableDateTimeSerializer();
        var date = new DateTime(1996, 7, 4, 0, 0, 0, DateTimeKind.Utc);

        Assert.Equal(date, Deserialize(serializer, new BsonDateTime(date)));
        Assert.Equal(date, Deserialize(serializer, new BsonString("1996-07-04 00:00:00.000")));
        Assert.Null(Deserialize(serializer, new BsonString("not a date")));
        Assert.Null(Deserialize(serializer, BsonNull.Value));
    }

    [Fact]
    public void DateTimeSerializer_WhenValueIsDamaged_ShouldReturnMinValue()
    {
        Assert.Equal(DateTime.MinValue, Deserialize(new TolerantDateTimeSerializer(), new BsonString("not a date")));
    }

    private static T Deserialize<T>(IBsonSerializer<T> serializer, BsonValue value)
    {
        using var reader = new BsonDocumentReader(new BsonDocument("value", value));

        reader.ReadStartDocument();
        reader.ReadName();

        return serializer.Deserialize(BsonDeserializationContext.CreateRoot(reader));
    }
}
