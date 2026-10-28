using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer.Logging;

/// <summary>
/// Stores entity changes of both databases in the Northwind "logs" collection
/// and duplicates them as structured log events.
/// </summary>
public class EntityChangeLogger(
    IEntityChangeLogRepository logRepository,
    ILogger<EntityChangeLogger> logger) : IEntityChangeLogger
{
    public async Task LogAsync(IReadOnlyCollection<EntityChange> changes)
    {
        var logs = CreateLogs(changes);

        try
        {
            await logRepository.AddRangeAsync(logs);
        }
        catch (Exception ex)
        {
            LogStoreFailure(ex, logs);
        }
    }

    public void Log(IReadOnlyCollection<EntityChange> changes)
    {
        var logs = CreateLogs(changes);

        try
        {
            logRepository.AddRange(logs);
        }
        catch (Exception ex)
        {
            LogStoreFailure(ex, logs);
        }
    }

    private List<EntityChangeLogEntity> CreateLogs(IReadOnlyCollection<EntityChange> changes)
    {
        var date = DateTime.UtcNow;
        var logs = new List<EntityChangeLogEntity>(changes.Count);

        foreach (var change in changes)
        {
            var log = new EntityChangeLogEntity
            {
                Date = date,
                Database = change.Database,
                Action = change.Action,
                EntityType = change.EntityType,
                OldVersion = change.OldVersion,
                NewVersion = change.NewVersion,
            };

            logger.LogInformation(
                "Entity change: {Action} {EntityType} in {Database} at {Date}. Old: {@OldVersion}. New: {@NewVersion}",
                log.Action,
                log.EntityType,
                log.Database,
                log.Date,
                log.OldVersion?.ToString(),
                log.NewVersion?.ToString());

            logs.Add(log);
        }

        return logs;
    }

    // Logging must never break the business operation that produced the changes.
    private void LogStoreFailure(Exception ex, List<EntityChangeLogEntity> logs)
    {
        logger.LogError(ex, "Failed to store {Count} entity change logs", logs.Count);
    }
}
