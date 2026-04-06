using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class NameFilterStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Filter(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        if (!string.IsNullOrWhiteSpace(filter?.Name))
        {
            var name = filter.Name.Trim();
            query = query.Where(g => g.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }
}
