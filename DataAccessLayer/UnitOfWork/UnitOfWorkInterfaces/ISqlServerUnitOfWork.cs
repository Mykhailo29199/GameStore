using System.Data;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;

public interface ISqlServerUnitOfWork : IUnitOfWorkBase<GameStoreContext>
{
    IProductSqlRepository ProductRepository { get; }

    IPlatformSqlRepository PlatformRepository { get; }

    ICategorySqlRepository CategoryRepository { get; }

    IProductCategorySqlRepository ProductCategoryRepository { get; }

    IProductPlatformSqlRepository ProductPlatformRepository { get; }

    IProductFileSqlRepository ProductFileRepository { get; }

    ISupplierSqlRepository SupplierRepository { get; }

    IOrderSqlRepository OrderRepository { get; }

    IOrderDetailsSqlRepository OrderDetailsRepository { get; }

    ICommentSqlRepository CommentRepository { get; }

    IBanSqlRepository BanRepository { get; }

    IUserSqlRepository UserRepository { get; }

    IRoleSqlRepository RoleRepository { get; }

    ITranslationSqlRepository TranslationRepository { get; }

    /// <summary>
    /// Runs the action in a transaction with the given isolation level and commits it when the action succeeds.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, IsolationLevel isolationLevel);

    /// <inheritdoc cref="ExecuteInTransactionAsync{T}"/>
    Task ExecuteInTransactionAsync(Func<Task> action, IsolationLevel isolationLevel);
}
