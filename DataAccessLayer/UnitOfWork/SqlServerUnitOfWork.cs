using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.Repositories.SQLRepositories;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;

namespace DataAccessLayer.UnitOfWork;

public class SqlServerUnitOfWork : ISqlServerUnitOfWork
{
    private bool _disposed;

    private IProductSqlRepository? _gameRepository;
    private IPlatformSqlRepository? _platformRepository;
    private ICategorySqlRepository? _genreRepository;
    private IProductCategorySqlRepository? _gameGenreRepository;
    private IProductPlatformSqlRepository? _gamePlatformRepository;
    private IProductFileSqlRepository? _gameFileRepository;
    private ISupplierSqlRepository? _publisherRepository;
    private IOrderSqlRepository? _orderRepository;
    private IOrderDetailsSqlRepository? _orderGameRepository;
    private ICommentSqlRepository? _commentRepository;
    private IBanSqlRepository? _banRepository;

    public SqlServerUnitOfWork(GameStoreContext context)
    {
        Context = context;
    }

    public GameStoreContext Context { get; }

    public IProductSqlRepository GameRepository =>
        _gameRepository ??= new ProductSqlRepository(Context);

    public IPlatformSqlRepository PlatformRepository =>
        _platformRepository ??= new PlatformSqlRepository(Context);

    public ICategorySqlRepository GenreRepository =>
        _genreRepository ??= new CategorySqlRepository(Context);

    public IProductCategorySqlRepository GameGenreRepository =>
        _gameGenreRepository ??= new ProductCategorySqlRepository(Context);

    public IProductPlatformSqlRepository GamePlatformRepository =>
        _gamePlatformRepository ??= new ProductPlatformSqlRepository(Context);

    public IProductFileSqlRepository GameFileRepository =>
        _gameFileRepository ??= new ProductFileSqlRepository(Context);

    public ISupplierSqlRepository SupplierRepository =>
        _publisherRepository ??= new SupplierSqlRepository(Context);

    public IOrderSqlRepository OrderRepository =>
        _orderRepository ??= new OrderSqlRepository(Context);

    public IOrderDetailsSqlRepository OrderGameRepository =>
        _orderGameRepository ??= new OrderDetailsSqlRepository(Context);

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