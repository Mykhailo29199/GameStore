using BusinessLogicLayer.Models;

namespace BusinessLogicLayer.Services.Interaces;

public interface IGenreService
{
    Task CreateGenreAsync(GenreModel genreModel);

    Task<GenreModel> GetGenreByIdAsync(Guid genreId);

    Task<List<GenreModel>> GetAllGenresAsync();

    Task<IList<GenreModel>> GetGenresByParentGenreIdAsync(Guid parentGenreId);

    Task UpdateGenreAsync(GenreModel genreModel);

    Task DeleteGenreAsync(Guid id);

    Task<List<GameModel>> GetGamesByGenreIdAsync(Guid genreId);
}
