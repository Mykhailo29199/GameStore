using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;
using GameStore.Shared.Enums.FilterOptionsEnums;

namespace BusinessLogicLayer.Pipelines.Steps;

public class PaginationStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Filter(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        if (filter.PageCount == (int)PaginationOptions.All)
        {
            return query;
        }

        int pageSize = filter.PageCount;

        return query
            .Skip((filter.Page - 1) * pageSize)
            .Take(pageSize);
    }
}
