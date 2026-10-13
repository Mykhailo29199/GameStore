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
}
