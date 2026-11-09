using System.Linq.Expressions;
using DataAccessLayer.Entities.GameStoreSQLEntities.Localization;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class TranslationSqlRepository(GameStoreContext context) : ITranslationSqlRepository
{
    public async Task<List<LocalizedGameEntity>> GetLocalizedGamesAsync(string languageCode, IReadOnlyCollection<Guid> productIds)
    {
        return await context.GetLocalizedGames(languageCode)
            .Where(g => productIds.Contains(g.Id))
            .ToListAsync();
    }

    public async Task<List<CategoryTranslationEntity>> GetCategoryTranslationsAsync(string languageCode, IReadOnlyCollection<Guid> categoryIds)
    {
        return await context.CategoryTranslations.AsNoTracking()
            .Where(t => t.LanguageCode == languageCode && categoryIds.Contains(t.CategoryId))
            .ToListAsync();
    }

    public async Task<List<PlatformTranslationEntity>> GetPlatformTranslationsAsync(string languageCode, IReadOnlyCollection<Guid> platformIds)
    {
        return await context.PlatformTranslations.AsNoTracking()
            .Where(t => t.LanguageCode == languageCode && platformIds.Contains(t.PlatformId))
            .ToListAsync();
    }

    public async Task<List<SupplierTranslationEntity>> GetSupplierTranslationsAsync(string languageCode, IReadOnlyCollection<Guid> supplierIds)
    {
        return await context.SupplierTranslations.AsNoTracking()
            .Where(t => t.LanguageCode == languageCode && supplierIds.Contains(t.SupplierId))
            .ToListAsync();
    }

    public async Task LoadTranslationsAsync<TEntity, TTranslation>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TTranslation>>> translations)
        where TEntity : class
        where TTranslation : class
    {
        var entry = context.Entry(entity);

        // A new entity, for example a Northwind copy that is not saved yet, has no translations in the database.
        if (entry.State != EntityState.Added)
        {
            await entry.Collection(translations).LoadAsync();
        }
    }
}
