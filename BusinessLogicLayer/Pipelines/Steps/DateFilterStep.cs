using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using GameStore.Shared.Enums.FilterOptionsEnums;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Pipelines.Steps;

public class DateFilterStep : IGamePipelineStep
{
    public static DateTime GetFromDate(PublishDateOptions? option)
    {
        var now = DateTime.UtcNow;

        return option switch
        {
            PublishDateOptions.Last24Hours => now.AddDays(-1),
            PublishDateOptions.LastWeek => now.AddDays(-7),
            PublishDateOptions.LastMonth => now.AddMonths(-1),
            PublishDateOptions.LastYear => now.AddYears(-1),
            PublishDateOptions.AllTime => DateTime.MinValue,
            _ => throw new BadRequestException("Invalid date publishing option"),
        };
    }

    public IQueryable<ProductEntity> Filter(IQueryable<ProductEntity> query, GameFilterModel filter)
    {
        var fromDate = GetFromDate(filter.DatePublishing);

        return query.Where(g => g.CreatedAt >= fromDate);
    }
}
