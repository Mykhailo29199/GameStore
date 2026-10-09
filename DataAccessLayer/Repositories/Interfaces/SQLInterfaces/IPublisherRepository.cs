using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IPublisherRepository : IGenericRepository<PublisherEntity>
{
    Task<IList<GameEntity>> GetGamesByPublisherNameAsync(string publisherName);

    Task<GameEntity> GetGamesByPublisherIdAsync(Guid id);
}
