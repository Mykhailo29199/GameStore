using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class NameFilterStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Process(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Name) && filter.Name.Length >= 1)
        {
            query = query.Where(g => g.Name.Contains(filter.Name));
        }

        return query;
    }
}
