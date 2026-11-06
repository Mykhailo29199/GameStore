using System.Linq.Expressions;
using DataAccessLayer.Entities.GameStoreSQLEntities.Localization;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface ITranslationSqlRepository
{
    /// <summary>
    /// Reads the games through the SQL function <c>dbo.GetLocalizedGames</c>.
    /// </summary>
    Task<List<LocalizedGameEntity>> GetLocalizedGamesAsync(string languageCode, IReadOnlyCollection<Guid> productIds);

    Task<List<CategoryTranslationEntity>> GetCategoryTranslationsAsync(string languageCode, IReadOnlyCollection<Guid> categoryIds);

    Task<List<PlatformTranslationEntity>> GetPlatformTranslationsAsync(string languageCode, IReadOnlyCollection<Guid> platformIds);

    Task<List<SupplierTranslationEntity>> GetSupplierTranslationsAsync(string languageCode, IReadOnlyCollection<Guid> supplierIds);

    /// <summary>
    /// Loads the translations of a tracked entity, so they can be changed or removed through its collection.
    /// </summary>
    Task LoadTranslationsAsync<TEntity, TTranslation>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TTranslation>>> translations)
        where TEntity : class
        where TTranslation : class;
}
