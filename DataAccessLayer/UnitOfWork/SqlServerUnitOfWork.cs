using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.Repositories.SQLRepositories;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;

namespace DataAccessLayer.UnitOfWork;

public class SqlServerUnitOfWork : ISqlServerUnitOfWork
{
    private bool _disposed;

    private IProductSqlRepository? _productRepository;

    private IPlatformSqlRepository? _platformRepository;
    private ICategorySqlRepository? _categoryRepository;
    private IProductCategorySqlRepository? _productCategoryRepository;
    private IProductPlatformSqlRepository? _productPlatformRepository;
    private IProductFileSqlRepository? _productFileRepository;
    private ISupplierSqlRepository? _supplierRepository;
    private IOrderSqlRepository? _orderRepository;
    private IOrderDetailsSqlRepository? _orderDetailsRepository;
    private ICommentSqlRepository? _commentRepository;
    private IBanSqlRepository? _banRepository;

    public SqlServerUnitOfWork(GameStoreContext context)
    {
        Context = context;
    }

    public GameStoreContext Context { get; }

    public IProductSqlRepository ProductRepository =>
        _productRepository ??= new ProductSqlRepository(Context);

    public IPlatformSqlRepository PlatformRepository =>
        _platformRepository ??= new PlatformSqlRepository(Context);

    public ICategorySqlRepository CategoryRepository =>
        _categoryRepository ??= new CategorySqlRepository(Context);

    public IProductCategorySqlRepository ProductCategoryRepository =>
        _productCategoryRepository ??= new ProductCategorySqlRepository(Context);

    public IProductPlatformSqlRepository ProductPlatformRepository =>
        _productPlatformRepository ??= new ProductPlatformSqlRepository(Context);

    public IProductFileSqlRepository ProductFileRepository =>
        _productFileRepository ??= new ProductFileSqlRepository(Context);

    public ISupplierSqlRepository SupplierRepository =>
        _supplierRepository ??= new SupplierSqlRepository(Context);

    public IOrderSqlRepository OrderRepository =>
        _orderRepository ??= new OrderSqlRepository(Context);

    public IOrderDetailsSqlRepository OrderDetailsRepository =>
        _orderDetailsRepository ??= new OrderDetailsSqlRepository(Context);

    public ICommentSqlRepository CommentRepository =>
        _commentRepository ??= new CommentSqlRepository(Context);

    public IBanSqlRepository BanRepository =>
        _banRepository ??= new BanSqlRepository(Context);

    public async Task CreateTransactionAsync() =>
        await Context.Database.BeginTransactionAsync();

    public async Task CommitAsync() =>
        await Context.Database.CommitTransactionAsync();

    public async Task RollbackAsync() =>
        await Context.Database.RollbackTransactionAsync();

    public async Task SaveAsync() =>
        await Context.SaveChangesAsync();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Context.Dispose();
            }

            _disposed = true;
        }
    }
}