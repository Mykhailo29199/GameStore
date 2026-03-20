using DataAccessLayer.Repositories;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;

namespace DataAccessLayer.UnitOfWork;

public class UnitOfWork : IUnitOfWork<GameStoreContext>
{
    private bool _disposed;

    private IGameRepository? _gameRepository;
    private IPlatformRepository? _platformRepository;
    private IGenreRepository? _genreRepository;
    private IGameGenreRepository? _gameGenreRepository;
    private IGamePlatformRepository? _gamePlatformRepository;
    private IGameFileRepository? _gameFileRepository;
    private IPublisherRepository? _publisherRepository;
    private IOrderRepository? _orderRepository;
    private IOrderGameRepository? _orderGameRepository;
    private ICommentRepository? _commentRepository;
    private IBanRepository? _banRepository;

    public UnitOfWork(GameStoreContext context)
    {
        Context = context;
    }

    public GameStoreContext Context { get; }

    public IGameRepository GameRepository =>
        _gameRepository ??= new GameRepository(Context);

    public IPlatformRepository PlatformRepository =>
        _platformRepository ??= new PlatformRepository(Context);

    public IGenreRepository GenreRepository =>
        _genreRepository ??= new GenreRepository(Context);

    public IGameGenreRepository GameGenreRepository =>
        _gameGenreRepository ??= new GameGenreRepository(Context);

    public IGamePlatformRepository GamePlatformRepository =>
        _gamePlatformRepository ??= new GamePlatformRepository(Context);

    public IGameFileRepository GameFileRepository =>
        _gameFileRepository ??= new GameFileRepository(Context);

    public IPublisherRepository PublisherRepository =>
        _publisherRepository ??= new PublisherRepository(Context);

    public IOrderRepository OrderRepository =>
        _orderRepository ??= new OrderRepository(Context);

    public IOrderGameRepository OrderGameRepository =>
        _orderGameRepository ??= new OrderGameRepository(Context);

    public ICommentRepository CommentRepository =>
        _commentRepository ??= new CommentRepository(Context);

    public IBanRepository BanRepository =>
        _banRepository ??= new BanRepository(Context);

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