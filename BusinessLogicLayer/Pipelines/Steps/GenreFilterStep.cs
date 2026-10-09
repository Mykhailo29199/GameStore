using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class GenreFilterStep : IGamePipelineStep
{
    public IQueryable<ProductEntity> Filter(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        if (filter.Genres?.Count > 0)
        {
            query = query.Where(g =>
                g.ProductCategories.Any(gg => filter.Genres.Contains(gg.CategoryId)));
        }

        return query;
    }
}
