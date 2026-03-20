using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.UnitOfWork;

public interface IUnitOfWork<out TGameStoreContext> : IDisposable
    where TGameStoreContext : DbContext
{
    IGameRepository GameRepository { get; }

    IPlatformRepository PlatformRepository { get; }

    IGenreRepository GenreRepository { get; }

    IGameGenreRepository GameGenreRepository { get; }

    IGamePlatformRepository GamePlatformRepository { get; }

    IGameFileRepository GameFileRepository { get; }

    IPublisherRepository PublisherRepository { get; }

    IOrderRepository OrderRepository { get; }

    IOrderGameRepository OrderGameRepository { get; }

    ICommentRepository CommentRepository { get; }

    IBanRepository BanRepository { get; }

    TGameStoreContext Context { get; }

    Task CreateTransactionAsync();

    Task CommitAsync();

    Task RollbackAsync();

    Task SaveAsync();
}
