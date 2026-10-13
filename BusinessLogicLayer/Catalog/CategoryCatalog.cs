using BusinessLogicLayer.Catalog.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Mongo;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Catalog;

public class CategoryCatalog(ISqlServerUnitOfWork sqlUnitOfWork, IMongoDBUnitOfWork mongoUnitOfWork) : ICategoryCatalog
{
    private static readonly NorthwindResolverOptions<CategoryEntity, DataAccessLayer.Entities.NorthwindMDBEntities.CategoryEntity> ResolverOptions = new(
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
        var name = NorthwindResolver.Normalize(category.Name);
        var northwindCategories = await mongoUnitOfWork.CategoryRepository.GetAllAsync();

        return northwindCategories.Any(c =>
            MongoIdConverter.ToGuid(c.InternalId) == category.Id || NorthwindResolver.Normalize(c.Name) == name);
    }

    public async Task<NorthwindResolution<CategoryEntity>> ResolveNorthwindAsync()
    {
        var sqlCategories = await sqlUnitOfWork.GenreRepository.GetAllIncludingDeleted().ToListAsync();
        var northwindCategories = await mongoUnitOfWork.CategoryRepository.GetAllAsync();

        return NorthwindResolver.Resolve(sqlCategories, northwindCategories, ResolverOptions);
    }

    private async Task<CategoryEntity?> GetNorthwindOnlyAsync(Guid id)
    {
        if (!MongoIdConverter.TryGetObjectId(id, out _))
        {
            return null;
        }

        var northwind = await ResolveNorthwindAsync();

        return northwind.NorthwindOnly.FirstOrDefault(c => c.Id == id);
    }
}
