using MongoDB.Bson;

namespace DataAccessLayer.Mongo;

/// <summary>
/// Converts Northwind ObjectId identifiers into Game Store Guid identifiers and back.
/// The 12 ObjectId bytes are stored in the tail of a Guid whose first 4 bytes are zero,
/// so every Northwind entity gets a stable Guid that can be decoded without a lookup table.
/// </summary>
public static class MongoIdConverter
{
    private const int GuidLength = 16;
    private const int PrefixLength = GuidLength - 12;

    public static Guid ToGuid(ObjectId objectId)
    {
        var bytes = new byte[GuidLength];
        objectId.ToByteArray().CopyTo(bytes, PrefixLength);

        return new Guid(bytes);
    }

    public static bool TryGetObjectId(Guid id, out ObjectId objectId)
    {
        var bytes = id.ToByteArray();

        if (bytes.Take(PrefixLength).Any(b => b != 0))
        {
            objectId = ObjectId.Empty;
            return false;
        }

        objectId = new ObjectId(bytes.Skip(PrefixLength).ToArray());
        return true;
    }
}
