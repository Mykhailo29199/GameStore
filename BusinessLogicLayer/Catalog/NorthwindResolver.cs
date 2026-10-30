using DataAccessLayer.Mongo;
using MongoDB.Bson;

namespace BusinessLogicLayer.Catalog;

/// <summary>
/// Implements duplicate management between the two databases:
/// a Game Store entity always wins over a Northwind one (matched by Guid of a copied entity or by unique name),
/// and among Northwind duplicates the first found document is used.
/// </summary>
public static class NorthwindResolver
{
    public static NorthwindResolution<TEntity> Resolve<TEntity, TDocument>(
        IEnumerable<TEntity> sqlEntitiesIncludingDeleted,
        IEnumerable<TDocument> northwindDocuments,
        NorthwindResolverOptions<TEntity, TDocument> options)
    {
        var sqlEntities = sqlEntitiesIncludingDeleted.ToList();

        var sqlById = sqlEntities.ToDictionary(options.GetId);
        var sqlByName = sqlEntities
            .Where(e => !string.IsNullOrWhiteSpace(options.GetName(e)))
            .GroupBy(e => Normalize(options.GetName(e)))
            .ToDictionary(g => g.Key, g => g.OrderBy(options.IsDeleted).First());

        var northwindOnly = new List<TEntity>();
        var firstByName = new Dictionary<string, TEntity>();
        var byLegacyId = new Dictionary<int, TEntity>();

        foreach (var document in northwindDocuments)
        {
            var converted = options.Convert(document);
            var name = options.GetName(converted);

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var normalizedName = Normalize(name);

            TEntity effective;

            if (sqlById.TryGetValue(options.GetId(converted), out var copy))
            {
                effective = copy;
            }
            else if (sqlByName.TryGetValue(normalizedName, out var duplicate))
            {
                effective = duplicate;
            }
            else if (firstByName.TryGetValue(normalizedName, out var firstFound))
            {
                effective = firstFound;
            }
            else
            {
                firstByName[normalizedName] = converted;
                northwindOnly.Add(converted);
                effective = converted;
            }

            var legacyId = options.GetLegacyId(document);

            if (legacyId.HasValue && !options.IsDeleted(effective))
            {
                byLegacyId.TryAdd(legacyId.Value, effective);
            }
        }

        return new NorthwindResolution<TEntity>(northwindOnly, byLegacyId);
    }

    /// <summary>
    /// Loads only the records that decide how the given Northwind documents are resolved: Northwind documents with the same
    /// id or name (a game key for products) and Game Store entities, including deleted ones, copied from them or having the
    /// same name. Duplicates are matched only by id and name, so resolving these records gives the same result for the
    /// given documents as resolving the whole collections.
    /// </summary>
    public static async Task<(List<TEntity> SqlEntities, List<TDocument> Documents)> LoadRelatedAsync<TEntity, TDocument>(
        IReadOnlyCollection<TDocument> documents,
        Func<TDocument, ObjectId> getObjectId,
        Func<TDocument, string?> getName,
        Func<List<ObjectId>, List<string>, Task<List<TDocument>>> findDocumentsAsync,
        Func<List<Guid>, List<string>, Task<List<TEntity>>> findSqlEntitiesAsync)
    {
        if (documents.Count == 0)
        {
            return ([], []);
        }

        var names = documents.Select(getName)
            .OfType<string>()
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var related = await findDocumentsAsync(documents.Select(getObjectId).ToList(), names);
        var relatedIds = related.Select(document => MongoIdConverter.ToGuid(getObjectId(document))).ToList();

        return (await findSqlEntitiesAsync(relatedIds, names), related);
    }

    public static string Normalize(string? value)
    {
        return value?.Trim().ToUpperInvariant() ?? string.Empty;
    }
}
