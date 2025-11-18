using DataAccessLayer.Repositories;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.UnitOfWork;

public class UnitOfWork(GameStoreContext context) : IUnitOfWork<GameStoreContext>
{
    private IGameRepository? _gameRepository;
    private IPlatformRepository? _platformRepository;
    private IGenreRepository? _genreRepository;

    public GameStoreContext Context { get; } = context;

    public IGameRepository GameRepository => _gameRepository ??= new GameRepository(Context);

    public IPlatformRepository PlatformRepository => _platformRepository ??= new PlatformRepository(Context);

    public IGenreRepository GenreRepository => _genreRepository ??= new GenreRepository(Context);

    public async Task CreateTransactionAsync()
    {
        await Context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        await Context.Database.CommitTransactionAsync();
    }

    public async Task RollbackAsync()
    {
        await Context.Database.RollbackTransactionAsync();
    }

    public async Task SaveAsync()
    {
        await Context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
