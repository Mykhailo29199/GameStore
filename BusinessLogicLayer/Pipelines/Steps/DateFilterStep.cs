using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class DateFilterStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Process(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        if (string.IsNullOrWhiteSpace(filter.DatePublishing))
        {
            return query;
        }

        var now = DateTime.UtcNow;

        DateTime fromDate = filter.DatePublishing switch
        {
            "Last 24 hours" => now.AddDays(-1),
            "Last week" => now.AddDays(-7),
            "Last month" => now.AddMonths(-1),
            "Last year" => now.AddYears(-1),
            _ => DateTime.MinValue,
        };

        return query.Where(g => g.CreatedAt >= fromDate);
    }
}
