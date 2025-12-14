using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.UnitOfWork;

public class UnitOfWork(
    GameStoreContext context,
    IGameRepository gameRepository,
    IPlatformRepository platformRepository,
    IGenreRepository genreRepository,
    IGameGenreRepository gameGenreRepository,
    IGameFileRepository gameFileRepository,
    IGamePlatformRepository gamePlatformRepository) : IUnitOfWork<GameStoreContext>, IDisposable
{
    private bool _disposed;

    public GameStoreContext Context { get; } = context;

    public IGameRepository GameRepository { get; } = gameRepository;

    public IPlatformRepository PlatformRepository { get; } = platformRepository;

    public IGenreRepository GenreRepository { get; } = genreRepository;

    public IGameGenreRepository GameGenreRepository { get; } = gameGenreRepository;

    public IGamePlatformRepository GamePlatformRepository { get; } = gamePlatformRepository;

    public IGameFileRepository GameFileRepository { get; } = gameFileRepository;

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
        Context.Dispose();
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