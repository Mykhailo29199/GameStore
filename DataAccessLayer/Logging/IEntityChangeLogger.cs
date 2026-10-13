namespace DataAccessLayer.Logging;

public interface IEntityChangeLogger
{
    Task LogAsync(EntityChange change);
}
