using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.UnitOfWork;

public class UnitOfWork(
    GameStoreContext context,
    IGameRepository gameRepository,
    IPlatformRepository platformRepository,
    IGenreRepository genreRepository) : IUnitOfWork<GameStoreContext>, IDisposable
{
    public GameStoreContext Context { get; } = context;

    public IGameRepository GameRepository { get; } = gameRepository;

    public IPlatformRepository PlatformRepository { get; } = platformRepository;

    public IGenreRepository GenreRepository { get; } = genreRepository;

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