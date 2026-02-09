using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IPublisherRepository : IGenericRepository<PublisherEntity>
{
    Task<IList<GameEntity>> GetGamesByPublisherNameAsync(string publisherName);
}
