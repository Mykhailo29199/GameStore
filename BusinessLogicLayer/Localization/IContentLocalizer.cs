using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace BusinessLogicLayer.Localization;

/// <summary>
/// Shows and saves entity data in the language of the current request.
/// English data is stored in the main tables, other languages in the translation tables.
/// </summary>
public interface IContentLocalizer
{
    bool IsDefaultLanguage { get; }

    Task LocalizeGamesAsync(IReadOnlyCollection<GameModel> games);

    Task LocalizeGenresAsync(IReadOnlyCollection<GenreModel> genres);

    Task LocalizePlatformsAsync(IReadOnlyCollection<PlatformModel> platforms);

    Task LocalizePublishersAsync(IReadOnlyCollection<PublisherModel> publishers);

    Task SaveGameTranslationAsync(ProductEntity game, string name, string? description);

    Task SaveGenreTranslationAsync(CategoryEntity genre, string name);

    Task SavePlatformTranslationAsync(PlatformEntity platform, string type);

    Task SavePublisherTranslationAsync(SupplierEntity publisher, string? description);

    Task RemoveTranslationsAsync(ProductEntity game);

    Task RemoveTranslationsAsync(CategoryEntity genre);

    Task RemoveTranslationsAsync(SupplierEntity publisher);
}
