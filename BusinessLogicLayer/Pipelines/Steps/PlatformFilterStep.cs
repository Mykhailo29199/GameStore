using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class PlatformFilterStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Filter(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        if (filter.Platforms?.Count > 0)
        {
            query = query.Where(g =>
                g.GamePlatforms.Any(gp => filter.Platforms.Contains(gp.PlatformId)));
        }

        return query;
    }
}
