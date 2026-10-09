using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class PriceFilterStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Filter(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(g => g.Price >= filter.MinPrice);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(g => g.Price <= filter.MaxPrice);
        }

        return query;
    }
}
