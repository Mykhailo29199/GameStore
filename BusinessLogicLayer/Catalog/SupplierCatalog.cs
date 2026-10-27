using BusinessLogicLayer.Catalog.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Mongo;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Northwind = DataAccessLayer.Entities.NorthwindMDBEntities;

namespace BusinessLogicLayer.Catalog;

public class SupplierCatalog(ISqlServerUnitOfWork sqlUnitOfWork, IMongoDBUnitOfWork mongoUnitOfWork) : ISupplierCatalog
{
    private static readonly NorthwindResolverOptions<SupplierEntity, Northwind.SupplierEntity> ResolverOptions = new(
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
        var name = companyName.Trim();

        // The default SQL Server collation ignores case, as the duplicate check does.
        var sqlSupplier = await sqlUnitOfWork.SupplierRepository.FirstOrDefaultAsync(s => s.CompanyName.Trim() == name);

        if (sqlSupplier is not null)
        {
            return sqlSupplier;
        }

        var documents = await mongoUnitOfWork.SupplierRepository.GetByIdsOrNamesAsync([], [name]);
        var northwind = await ResolveSubsetAsync(documents);
        var normalizedName = NorthwindResolver.Normalize(name);

        return northwind.NorthwindOnly.FirstOrDefault(s => NorthwindResolver.Normalize(s.CompanyName) == normalizedName);
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
        ObjectId[] ids = MongoIdConverter.TryGetObjectId(supplier.Id, out var objectId) ? [objectId] : [];

        return await mongoUnitOfWork.SupplierRepository.ExistsByIdOrNameAsync(ids, supplier.CompanyName);
    }

    public async Task<NorthwindResolution<SupplierEntity>> ResolveNorthwindAsync()
    {
        var sqlSuppliers = await sqlUnitOfWork.SupplierRepository.GetAllIncludingDeleted().ToListAsync();
        var northwindSuppliers = await mongoUnitOfWork.SupplierRepository.GetAllAsync();

        return NorthwindResolver.Resolve(sqlSuppliers, northwindSuppliers, ResolverOptions);
    }

    public async Task<NorthwindResolution<SupplierEntity>> ResolveNorthwindAsync(IReadOnlyCollection<int> legacyIds)
    {
        var ids = legacyIds.Select(id => (int?)id).ToList();
        var documents = await mongoUnitOfWork.SupplierRepository.FindAsync(s => ids.Contains(s.SupplierID));

        return await ResolveSubsetAsync(documents);
    }

    private async Task<SupplierEntity?> GetNorthwindOnlyAsync(Guid id)
    {
        if (!MongoIdConverter.TryGetObjectId(id, out var objectId)
            || await mongoUnitOfWork.SupplierRepository.GetByIdAsync(objectId) is not { } document)
        {
            return null;
        }

        var northwind = await ResolveSubsetAsync([document]);

        return northwind.NorthwindOnly.FirstOrDefault(s => s.Id == id);
    }

    private async Task<NorthwindResolution<SupplierEntity>> ResolveSubsetAsync(List<Northwind.SupplierEntity> documents)
    {
        var (sqlSuppliers, related) = await NorthwindResolver.LoadRelatedAsync<SupplierEntity, Northwind.SupplierEntity>(
            documents,
            s => s.InternalId,
            s => s.CompanyName,
            mongoUnitOfWork.SupplierRepository.GetByIdsOrNamesAsync,
            FindSqlSuppliersAsync);

        return NorthwindResolver.Resolve(sqlSuppliers, related, ResolverOptions);
    }

    private async Task<List<SupplierEntity>> FindSqlSuppliersAsync(List<Guid> ids, List<string> names)
    {
        // The default SQL Server collation ignores case, as the duplicate check does.
        return await sqlUnitOfWork.SupplierRepository.GetAllIncludingDeleted()
            .Where(s => ids.Contains(s.Id) || names.Contains(s.CompanyName.Trim()))
            .ToListAsync();
    }
}
