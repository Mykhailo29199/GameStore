using BusinessLogicLayer.Models.GameModels;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Pipelines.Interfaces;

public interface IGamePipelineStep
{
    IQueryable<GameEntity> Process(IQueryable<GameEntity> query, GameFilterModel filter);
}
