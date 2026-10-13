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

    public static string Normalize(string? value)
    {
        return value?.Trim().ToUpperInvariant() ?? string.Empty;
    }
}
