using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class PlatformFilterStep : IGamePipelineStep
{
    public IQueryable<ProductEntity> Filter(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        if (filter.Platforms?.Count > 0)
        {
            query = query.Where(g =>
                g.ProductPlatforms.Any(gp => filter.Platforms.Contains(gp.PlatformId)));
        }

        return query;
    }
}
