using MongoDB.Bson;

namespace DataAccessLayer.Logging;

public record EntityChange(
    string Database,
    string Action,
    string EntityType,
    BsonDocument? OldVersion,
    BsonDocument? NewVersion)
{
    public const string CreateAction = "Create";
    public const string UpdateAction = "Update";
    public const string DeleteAction = "Delete";

    public const string GameStoreDatabase = "GameStore";
    public const string NorthwindDatabase = "Northwind";

    /// <summary>
    /// Creates an update that keeps only the key fields and the fields whose values changed,
    /// so a small update such as a new view count does not store the whole entity twice.
    /// </summary>
    public static EntityChange Update(
        string database,
        string entityType,
        BsonDocument oldVersion,
        BsonDocument newVersion,
        IReadOnlyCollection<string> keyFields)
    {
        var oldFields = new BsonDocument();
        var newFields = new BsonDocument();

        foreach (var name in oldVersion.Names.Union(newVersion.Names))
        {
            var hasOld = oldVersion.TryGetValue(name, out var oldValue);
            var hasNew = newVersion.TryGetValue(name, out var newValue);

            if (!keyFields.Contains(name) && hasOld == hasNew && Equals(oldValue, newValue))
            {
                continue;
            }

            if (hasOld)
            {
                oldFields[name] = oldValue;
            }

            if (hasNew)
            {
                newFields[name] = newValue;
            }
        }

        return new EntityChange(database, UpdateAction, entityType, oldFields, newFields);
    }
}
