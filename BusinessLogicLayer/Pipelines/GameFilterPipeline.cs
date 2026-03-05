using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Pipelines;

public class GameFilterPipeline(IEnumerable<IGamePipelineStep> steps)
{
    private readonly IEnumerable<IGamePipelineStep> _steps = steps;

    public IQueryable<GameEntity> Execute(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        foreach (var step in _steps)
        {
            query = step.Process(query, filter);
        }

        return query;
    }
}
