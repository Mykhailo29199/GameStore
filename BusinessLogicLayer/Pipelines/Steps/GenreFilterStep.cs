using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Pipelines.Interfaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Pipelines.Steps;

public class GenreFilterStep : IGamePipelineStep
{
    public IQueryable<GameEntity> Filter(IQueryable<GameEntity> query, GameFilterModel filter)
    {
        if (filter.Genres?.Count > 0)
        {
            query = query.Where(g =>
                g.GameGenres.Any(gg => filter.Genres.Contains(gg.GenreId)));
        }

        return query;
    }
}
