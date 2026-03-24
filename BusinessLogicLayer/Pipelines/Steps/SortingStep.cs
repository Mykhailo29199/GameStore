using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;
using GameStore.Shared.Enums.FilterOptionsEnums;
using GameStore.Shared.Extensions;

namespace BusinessLogicLayer.Pipelines.Steps;

public class SortingStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Process(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        var sortOption = string.IsNullOrWhiteSpace(filter.Sort)
            ? SortingOptions.MostPopular
            : filter.Sort.ToEnum<SortingOptions>();

        return sortOption switch
        {
            SortingOptions.MostPopular => query.OrderByDescending(g => g.Views),
            SortingOptions.MostCommented => query.OrderByDescending(g => g.Comments.Count),
            SortingOptions.PriceAsc => query.OrderBy(g => g.Price),
            SortingOptions.PriceDesc => query.OrderByDescending(g => g.Price),
            SortingOptions.New => query.OrderByDescending(g => g.CreatedAt),
            _ => query.OrderBy(g => g.Name),
        };
    }
}
