namespace DataAccessLayer.Logging;

public interface IEntityChangeLogger
{
    Task LogAsync(IReadOnlyCollection<EntityChange> changes);

    void Log(IReadOnlyCollection<EntityChange> changes);
}
