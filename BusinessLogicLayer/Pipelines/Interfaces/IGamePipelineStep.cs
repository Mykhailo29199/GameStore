using BusinessLogicLayer.Models.GameModels;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines.Interfaces;

public interface IGamePipelineStep
{
    IQueryable<ProductEntity> Filter(IQueryable<ProductEntity> query, GameFilterModel filter);
}
