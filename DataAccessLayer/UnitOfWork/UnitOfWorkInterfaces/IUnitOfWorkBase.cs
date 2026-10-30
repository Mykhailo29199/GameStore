using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;

public interface IUnitOfWorkBase<out TGameStoreContext> : IDisposable
    where TGameStoreContext : DbContext
{
    TGameStoreContext Context { get; }

    Task CreateTransactionAsync();

    Task CommitAsync();

    Task RollbackAsync();

    Task SaveAsync();
}
