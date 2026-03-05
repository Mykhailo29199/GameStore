using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class SortingStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Process(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        return filter.Sort switch
        {
            "Most popular" => query.OrderByDescending(g => g.Views),
            "Most commented" => query.OrderByDescending(g => g.Comments.Count),
            "Price ASC" => query.OrderBy(g => g.Price),
            "Price DESC" => query.OrderByDescending(g => g.Price),
            "New" => query.OrderByDescending(g => g.CreatedAt),
            _ => query.OrderBy(g => g.Name),
        };
    }
}
