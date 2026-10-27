using BusinessLogicLayer.Catalog.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Mongo;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Northwind = DataAccessLayer.Entities.NorthwindMDBEntities;

namespace BusinessLogicLayer.Catalog;

public class CategoryCatalog(ISqlServerUnitOfWork sqlUnitOfWork, IMongoDBUnitOfWork mongoUnitOfWork) : ICategoryCatalog
{
    private static readonly NorthwindResolverOptions<CategoryEntity, Northwind.CategoryEntity> ResolverOptions = new(
        NorthwindEntityConverter.ToCategory,
        c => c.CategoryID,
        c => c.Id,
        c => c.Name,
        c => c.IsDeleted);

    public async Task<List<CategoryEntity>> GetAllAsync()
    {
        var sqlCategories = await sqlUnitOfWork.GenreRepository.GetAllAsync();
        var northwind = await ResolveNorthwindAsync();

        return sqlCategories.Concat(northwind.NorthwindOnly).ToList();
    }

    public async Task<CategoryEntity?> GetByIdAsync(Guid id)
    {
        var sqlCategory = await sqlUnitOfWork.GenreRepository.FirstOrDefaultAsync(c => c.Id == id);

        return sqlCategory ?? await GetNorthwindOnlyAsync(id);
    }

    public async Task<CategoryEntity> EnsureInGameStoreAsync(Guid id)
    {
        var sqlCategory = await sqlUnitOfWork.GenreRepository.FirstOrDefaultAsync(c => c.Id == id);

        if (sqlCategory is not null)
        {
            return sqlCategory;
        }

        var northwindCategory = await GetNorthwindOnlyAsync(id)
            ?? throw new NotFoundException($"Genre with Id {id} does not exist.");

        await sqlUnitOfWork.GenreRepository.AddAsync(northwindCategory);

        return northwindCategory;
    }

    public async Task<bool> HasNorthwindCounterpartAsync(CategoryEntity category)
    {
        ObjectId[] ids = MongoIdConverter.TryGetObjectId(category.Id, out var objectId) ? [objectId] : [];

        return await mongoUnitOfWork.CategoryRepository.ExistsByIdOrNameAsync(ids, category.Name);
    }

    public async Task<NorthwindResolution<CategoryEntity>> ResolveNorthwindAsync()
    {
        var sqlCategories = await sqlUnitOfWork.GenreRepository.GetAllIncludingDeleted().ToListAsync();
        var northwindCategories = await mongoUnitOfWork.CategoryRepository.GetAllAsync();

        return NorthwindResolver.Resolve(sqlCategories, northwindCategories, ResolverOptions);
    }

    public async Task<NorthwindResolution<CategoryEntity>> ResolveNorthwindAsync(IReadOnlyCollection<int> legacyIds)
    {
        var ids = legacyIds.Select(id => (int?)id).ToList();
        var documents = await mongoUnitOfWork.CategoryRepository.FindAsync(c => ids.Contains(c.CategoryID));

        return await ResolveSubsetAsync(documents);
    }

    private async Task<CategoryEntity?> GetNorthwindOnlyAsync(Guid id)
    {
        if (!MongoIdConverter.TryGetObjectId(id, out var objectId)
            || await mongoUnitOfWork.CategoryRepository.GetByIdAsync(objectId) is not { } document)
        {
            return null;
        }

        var northwind = await ResolveSubsetAsync([document]);

        return northwind.NorthwindOnly.FirstOrDefault(c => c.Id == id);
    }

    private async Task<NorthwindResolution<CategoryEntity>> ResolveSubsetAsync(List<Northwind.CategoryEntity> documents)
    {
        var (sqlCategories, related) = await NorthwindResolver.LoadRelatedAsync<CategoryEntity, Northwind.CategoryEntity>(
            documents,
            c => c.InternalId,
            c => c.Name,
            mongoUnitOfWork.CategoryRepository.GetByIdsOrNamesAsync,
            FindSqlCategoriesAsync);

        return NorthwindResolver.Resolve(sqlCategories, related, ResolverOptions);
    }

    private async Task<List<CategoryEntity>> FindSqlCategoriesAsync(List<Guid> ids, List<string> names)
    {
        // The default SQL Server collation ignores case, as the duplicate check does.
        return await sqlUnitOfWork.GenreRepository.GetAllIncludingDeleted()
            .Where(c => ids.Contains(c.Id) || names.Contains(c.Name.Trim()))
            .ToListAsync();
    }
}
