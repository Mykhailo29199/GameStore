using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using BusinessLogicLayer.Pipelines.Steps;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines;

/// <summary>
/// Filters, sorts and pages games. The steps are separate, so filtering and sorting can run in the database
/// and paging can run after the games of both databases are merged.
/// </summary>
public class GameFilterPipeline(IEnumerable<IGamePipelineStep> filterSteps, SortingStep sorting, PaginationStep pagination)
{
    public IQueryable<ProductEntity> Filter(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        foreach (var step in filterSteps)
        {
            query = step.Filter(query, filter);
        }

        return query;
    }

    public IQueryable<ProductEntity> Sort(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        return sorting.Filter(query, filter);
    }

    public IQueryable<ProductEntity> Paginate(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        return pagination.Filter(query, filter);
    }
}
