using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.UnitOfWork;

public class UnitOfWork(
    GameStoreContext context,
    IGameRepository gameRepository,
    IPlatformRepository platformRepository,
    IGenreRepository genreRepository,
    IGameGenreRepository gameGenreRepository,
    IGamePlatformRepository gamePlatformRepository) : IUnitOfWork<GameStoreContext>, IDisposable
{
    public GameStoreContext Context { get; } = context;

    public IGameRepository GameRepository { get; } = gameRepository;

    public IPlatformRepository PlatformRepository { get; } = platformRepository;

    public IGenreRepository GenreRepository { get; } = genreRepository;

    public IGameGenreRepository GameGenreRepository { get; } = gameGenreRepository;

    public IGamePlatformRepository GamePlatformRepository { get; } = gamePlatformRepository;

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
}