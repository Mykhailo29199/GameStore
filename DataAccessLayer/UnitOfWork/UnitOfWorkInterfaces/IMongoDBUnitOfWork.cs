using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

namespace DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;

public interface IMongoDBUnitOfWork
{
    IProductMongoDBRepository ProductRepository { get; }

    ICategoryMongoDBRepository CategoryRepository { get; }

    ISupplierMongoDBRepository SupplierRepository { get; }

    IOrderMongoDBRepository OrderRepository { get; }

    IOrderDetailsMongoDBRepository OrderDetailsRepository { get; }

    IShipperMongoDBRepository ShipperRepository { get; }
}
