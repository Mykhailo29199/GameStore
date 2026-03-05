using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class PaginationStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Process(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        if (filter.PageCount == "All")
        {
            return query;
        }

        int pageSize = int.Parse(filter.PageCount);

        return query
            .Skip((filter.Page - 1) * pageSize)
            .Take(pageSize);
    }
}
