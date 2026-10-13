namespace BusinessLogicLayer.Catalog;

public record NorthwindResolverOptions<TEntity, TDocument>(
    Func<TDocument, TEntity> Convert,
    Func<TDocument, int?> GetLegacyId,
    Func<TEntity, Guid> GetId,
    Func<TEntity, string?> GetName,
    Func<TEntity, bool> IsDeleted);
