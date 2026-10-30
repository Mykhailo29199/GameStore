using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using GameStore.Shared.Enums.FilterOptionsEnums;

namespace BusinessLogicLayer.Pipelines.Steps;

public class SortingStep : IGamePipelineStep
{
    public IQueryable<ProductEntity> Filter(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        var sortOption = filter.Sort ?? SortingOptions.MostPopular;

        return sortOption switch
        {
            SortingOptions.MostPopular => query.OrderByDescending(g => g.Views),
            SortingOptions.MostCommented => query.OrderByDescending(g => g.Comments.Count),
            SortingOptions.PriceAsc => query.OrderBy(g => g.UnitPrice),
            SortingOptions.PriceDesc => query.OrderByDescending(g => g.UnitPrice),
            SortingOptions.New => query.OrderByDescending(g => g.CreatedAt),
            _ => query.OrderBy(g => g.ProductName),
        };
    }
}
