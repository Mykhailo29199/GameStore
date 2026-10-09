using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class PublisherFilterStep : IGamePipelineStep
{
    public IQueryable<ProductEntity> Filter(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        if (filter.Publishers?.Count > 0)
        {
            query = query.Where(g => filter.Publishers.Contains(g.SupplierID));
        }

        return query;
    }
}
