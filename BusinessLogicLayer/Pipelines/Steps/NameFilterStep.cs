using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class NameFilterStep : IGamePipelineStep
{
    public IQueryable<ProductEntity> Filter(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        if (!string.IsNullOrWhiteSpace(filter?.Name))
        {
            var name = filter.Name.Trim();
            query = query.Where(g => g.ProductName.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }
}
