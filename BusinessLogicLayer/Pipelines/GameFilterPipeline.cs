using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines;

public class GameFilterPipeline(IEnumerable<IGamePipelineStep> steps)
{
    private readonly IEnumerable<IGamePipelineStep> _steps = steps;

    public IQueryable<ProductEntity> Execute(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        foreach (var step in _steps)
        {
            query = step.Filter(query, filter);
        }

        return query;
    }
}
