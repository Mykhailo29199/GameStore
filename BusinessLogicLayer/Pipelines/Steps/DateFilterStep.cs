using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities;
using GameStore.Shared.Enums.FilterOptionsEnums;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Pipelines.Steps;

public class DateFilterStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Filter(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        var now = DateTime.UtcNow;

        DateTime fromDate = filter.DatePublishing switch
        {
            PublichDateOptions.Last24Hours => now.AddDays(-1),
            PublichDateOptions.LastWeek => now.AddDays(-7),
            PublichDateOptions.LastMonth => now.AddMonths(-1),
            PublichDateOptions.LastYear => now.AddYears(-1),
            PublichDateOptions.AllTime => DateTime.MinValue,
            _ => throw new BadRequestException("Invalid date publishing option"),
        };

        return query.Where(g => g.CreatedAt >= fromDate);
    }
}
