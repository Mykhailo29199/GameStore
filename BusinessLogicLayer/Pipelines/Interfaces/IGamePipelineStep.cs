using BusinessLogicLayer.Models.GameModels;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines.Interfaces;

public interface IGamePipelineStep
{
    IQueryable<GameEntity> Filter(IQueryable<GameEntity> query, GameFilterModel filter);
}
