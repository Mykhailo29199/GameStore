using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class PublisherFilterStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Process(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        if (filter.Publishers?.Count > 0)
        {
            query = query.Where(g => filter.Publishers.Contains(g.PublisherId));
        }

        return query;
    }
}
