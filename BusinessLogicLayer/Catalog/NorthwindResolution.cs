namespace BusinessLogicLayer.Catalog;

/// <summary>
/// Result of merging Northwind documents with Game Store entities.
/// </summary>
/// <param name="NorthwindOnly">Northwind entities that have no Game Store counterpart, converted to Game Store entities.</param>
/// <param name="ByLegacyId">Effective (not deleted) entity for every Northwind numeric id, used to resolve references.</param>
public record NorthwindResolution<TEntity>(
    List<TEntity> NorthwindOnly,
    Dictionary<int, TEntity> ByLegacyId);
