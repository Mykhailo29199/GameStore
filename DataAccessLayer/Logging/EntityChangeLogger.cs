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
    public async Task LogAsync(EntityChange change)
    {
        var log = new EntityChangeLogEntity
        {
            Date = DateTime.UtcNow,
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

        try
        {
            await logRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            // Logging must never break the business operation that produced the change.
            logger.LogError(ex, "Failed to store entity change log for {EntityType}", log.EntityType);
        }
    }
}
