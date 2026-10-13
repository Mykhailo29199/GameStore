using BusinessLogicLayer.Catalog.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Mongo;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Catalog;

public class SupplierCatalog(ISqlServerUnitOfWork sqlUnitOfWork, IMongoDBUnitOfWork mongoUnitOfWork) : ISupplierCatalog
{
    private static readonly NorthwindResolverOptions<SupplierEntity, DataAccessLayer.Entities.NorthwindMDBEntities.SupplierEntity> ResolverOptions = new(
        NorthwindEntityConverter.ToSupplier,
        s => s.SupplierID,
        s => s.Id,
        s => s.CompanyName,
        s => s.IsDeleted);

    public async Task<List<SupplierEntity>> GetAllAsync()
    {
        var sqlSuppliers = await sqlUnitOfWork.SupplierRepository.GetAllAsync();
        var northwind = await ResolveNorthwindAsync();

        return sqlSuppliers.Concat(northwind.NorthwindOnly).ToList();
    }

    public async Task<SupplierEntity?> GetByIdAsync(Guid id)
    {
        var sqlSupplier = await sqlUnitOfWork.SupplierRepository.FirstOrDefaultAsync(s => s.Id == id);

        return sqlSupplier ?? await GetNorthwindOnlyAsync(id);
    }

    public async Task<SupplierEntity?> GetByCompanyNameAsync(string companyName)
    {
        var name = NorthwindResolver.Normalize(companyName);
        var suppliers = await GetAllAsync();

        return suppliers.FirstOrDefault(s => NorthwindResolver.Normalize(s.CompanyName) == name);
    }

    public async Task<SupplierEntity> EnsureInGameStoreAsync(Guid id)
    {
        var sqlSupplier = await sqlUnitOfWork.SupplierRepository.FirstOrDefaultAsync(s => s.Id == id);

        if (sqlSupplier is not null)
        {
            return sqlSupplier;
        }

        var northwindSupplier = await GetNorthwindOnlyAsync(id)
            ?? throw new NotFoundException($"Publisher with id '{id}' does not exist.");

        await sqlUnitOfWork.SupplierRepository.AddAsync(northwindSupplier);

        return northwindSupplier;
    }

    public async Task<bool> HasNorthwindCounterpartAsync(SupplierEntity supplier)
    {
        var name = NorthwindResolver.Normalize(supplier.CompanyName);
        var northwindSuppliers = await mongoUnitOfWork.SupplierRepository.GetAllAsync();

        return northwindSuppliers.Any(s =>
            MongoIdConverter.ToGuid(s.InternalId) == supplier.Id || NorthwindResolver.Normalize(s.CompanyName) == name);
    }

    public async Task<NorthwindResolution<SupplierEntity>> ResolveNorthwindAsync()
    {
        var sqlSuppliers = await sqlUnitOfWork.SupplierRepository.GetAllIncludingDeleted().ToListAsync();
        var northwindSuppliers = await mongoUnitOfWork.SupplierRepository.GetAllAsync();

        return NorthwindResolver.Resolve(sqlSuppliers, northwindSuppliers, ResolverOptions);
    }

    private async Task<SupplierEntity?> GetNorthwindOnlyAsync(Guid id)
    {
        if (!MongoIdConverter.TryGetObjectId(id, out _))
        {
            return null;
        }

        var northwind = await ResolveNorthwindAsync();

        return northwind.NorthwindOnly.FirstOrDefault(s => s.Id == id);
    }
}
