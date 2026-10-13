using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MongoDB.Bson;

namespace DataAccessLayer.Logging;

/// <summary>
/// Captures every entity change saved to the Game Store SQL database and writes it to the change log.
/// </summary>
public class EntityChangeLoggingInterceptor(IEntityChangeLogger changeLogger) : SaveChangesInterceptor
{
    private readonly List<EntityChange> _pendingChanges = [];

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        FlushAsync().GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await FlushAsync();
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _pendingChanges.Clear();
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _pendingChanges.Clear();
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private static BsonDocument ToDocument(PropertyValues values)
    {
        var document = new BsonDocument();

        foreach (var property in values.Properties)
        {
            document[property.Name] = ToBsonValue(values[property]);
        }

        return document;
    }

    private static BsonValue ToBsonValue(object? value)
    {
        return value switch
        {
            null => BsonNull.Value,
            string s => s,
            bool b => b,
            int i => i,
            long l => l,
            double d => d,
            decimal m => m,
            DateTime dt => dt,
            Guid g => g.ToString(),
            Enum e => e.ToString(),
            _ => value.ToString(),
        };
    }

    private static EntityChange CreateChange(EntityEntry entry, string action, BsonDocument? oldVersion, BsonDocument? newVersion)
    {
        return new EntityChange(
            EntityChange.GameStoreDatabase,
            action,
            entry.Metadata.ClrType.Name,
            oldVersion,
            newVersion);
    }

    private void CaptureChanges(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            var change = entry.State switch
            {
                EntityState.Added => CreateChange(entry, EntityChange.CreateAction, null, ToDocument(entry.CurrentValues)),
                EntityState.Modified => CreateChange(entry, EntityChange.UpdateAction, ToDocument(entry.OriginalValues), ToDocument(entry.CurrentValues)),
                EntityState.Deleted => CreateChange(entry, EntityChange.DeleteAction, ToDocument(entry.OriginalValues), null),
                EntityState.Detached or EntityState.Unchanged => null,
                _ => null,
            };

            if (change is not null)
            {
                _pendingChanges.Add(change);
            }
        }
    }

    private async Task FlushAsync()
    {
        var changes = _pendingChanges.ToList();
        _pendingChanges.Clear();

        foreach (var change in changes)
        {
            await changeLogger.LogAsync(change);
        }
    }
}
