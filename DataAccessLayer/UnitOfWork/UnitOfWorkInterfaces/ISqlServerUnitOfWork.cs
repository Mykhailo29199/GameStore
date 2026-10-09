using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;

public interface ISqlServerUnitOfWork : IDisposable
{
    IProductSqlRepository GameRepository { get; }

    IPlatformSqlRepository PlatformRepository { get; }

    ICategorySqlRepository GenreRepository { get; }

    IProductCategorySqlRepository GameGenreRepository { get; }

    IProductPlatformSqlRepository GamePlatformRepository { get; }

    IProductFileSqlRepository GameFileRepository { get; }

    ISupplierSqlRepository SupplierRepository { get; }

    IOrderSqlRepository OrderRepository { get; }

    IOrderDetailsSqlRepository OrderGameRepository { get; }

    ICommentSqlRepository CommentRepository { get; }

    IBanSqlRepository BanRepository { get; }

    GameStoreContext Context { get; }

    Task CreateTransactionAsync();

    Task CommitAsync();

    Task RollbackAsync();

    Task SaveAsync();
}
