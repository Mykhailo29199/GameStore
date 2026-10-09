using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class PriceFilterStep : IGamePipelineStep
{
    public IQueryable<ProductEntity> Filter(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(g => g.UnitPrice >= filter.MinPrice);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(g => g.UnitPrice <= filter.MaxPrice);
        }

        return query;
    }
}
