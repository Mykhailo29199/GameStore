using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using DataAccessLayer.Repositories.MongoDBRepositories;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using MongoDB.Driver;

namespace DataAccessLayer.UnitOfWork;

public class MongoDBUnitOfWork(IMongoDatabase database, IEntityChangeLogger changeLogger) : IMongoDBUnitOfWork
{
    private IProductMongoDBRepository? _productRepository;
    private ICategoryMongoDBRepository? _categoryRepository;
    private ISupplierMongoDBRepository? _supplierRepository;
    private IOrderMongoDBRepository? _orderRepository;
    private IOrderDetailsMongoDBRepository? _orderDetailsRepository;
    private IShipperMongoDBRepository? _shipperRepository;

    public IProductMongoDBRepository ProductRepository =>
        _productRepository ??= new ProductMongoDBRepository(database, changeLogger);

    public ICategoryMongoDBRepository CategoryRepository =>
        _categoryRepository ??= new CategoryMongoDBRepository(database, changeLogger);

    public ISupplierMongoDBRepository SupplierRepository =>
        _supplierRepository ??= new SupplierMongoDBRepository(database, changeLogger);

    public IOrderMongoDBRepository OrderRepository =>
        _orderRepository ??= new OrderMongoDBRepository(database, changeLogger);

    public IOrderDetailsMongoDBRepository OrderDetailsRepository =>
        _orderDetailsRepository ??= new OrderDetailsMongoDBRepository(database, changeLogger);

    public IShipperMongoDBRepository ShipperRepository =>
        _shipperRepository ??= new ShipperMongoDBRepository(database, changeLogger);
}
