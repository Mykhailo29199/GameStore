using System.Linq.Expressions;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Entities.GameStoreSQLEntities.Localization;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Localization;

namespace BusinessLogicLayer.Localization;

/// <summary>
/// A value without a translation stays in English, so the default language is shown when a translation is missing.
/// </summary>
public class ContentLocalizer(ISqlServerUnitOfWork unitOfWork, ICurrentLanguage currentLanguage) : IContentLocalizer
{
    public bool IsDefaultLanguage => currentLanguage.Code == Languages.Default;

    public async Task LocalizeGamesAsync(IReadOnlyCollection<GameModel> games)
    {
        if (IsDefaultLanguage || games.Count == 0)
        {
            return;
        }

        var localizedGames = (await unitOfWork.TranslationRepository.GetLocalizedGamesAsync(currentLanguage.Code, GetIds(games, g => g.Id)))
            .ToDictionary(g => g.Id);

        foreach (var game in games)
        {
            if (localizedGames.TryGetValue(game.Id, out var localized))
            {
                game.Name = localized.Name;
                game.Description = localized.Description;
            }
        }
    }

    public async Task LocalizeGenresAsync(IReadOnlyCollection<GenreModel> genres)
    {
        if (IsDefaultLanguage || genres.Count == 0)
        {
            return;
        }

        var translations = (await unitOfWork.TranslationRepository.GetCategoryTranslationsAsync(currentLanguage.Code, GetIds(genres, g => g.Id)))
            .ToDictionary(t => t.CategoryId);

        foreach (var genre in genres)
        {
            if (translations.TryGetValue(genre.Id, out var translation))
            {
                genre.Name = translation.Name;
            }
        }
    }

    public async Task LocalizePlatformsAsync(IReadOnlyCollection<PlatformModel> platforms)
    {
        if (IsDefaultLanguage || platforms.Count == 0)
        {
            return;
        }

        var translations = (await unitOfWork.TranslationRepository.GetPlatformTranslationsAsync(currentLanguage.Code, GetIds(platforms, p => p.Id)))
            .ToDictionary(t => t.PlatformId);

        foreach (var platform in platforms)
        {
            if (translations.TryGetValue(platform.Id, out var translation))
            {
                platform.Type = translation.Type;
            }
        }
    }

    public async Task LocalizePublishersAsync(IReadOnlyCollection<PublisherModel> publishers)
    {
        if (IsDefaultLanguage || publishers.Count == 0)
        {
            return;
        }

        var translations = (await unitOfWork.TranslationRepository.GetSupplierTranslationsAsync(currentLanguage.Code, GetIds(publishers, p => p.Id)))
            .ToDictionary(t => t.SupplierId);

        foreach (var publisher in publishers)
        {
            if (translations.TryGetValue(publisher.Id, out var translation))
            {
                publisher.Description = translation.Description ?? publisher.Description;
            }
        }
    }

    public async Task SaveGameTranslationAsync(ProductEntity game, string name, string? description)
    {
        var translation = await GetOrAddTranslationAsync(game, g => g.Translations, game.Translations);
        translation.Name = name;
        translation.Description = description;
    }

    public async Task SaveGenreTranslationAsync(CategoryEntity genre, string name)
    {
        var translation = await GetOrAddTranslationAsync(genre, g => g.Translations, genre.Translations);
        translation.Name = name;
    }

    public async Task SavePlatformTranslationAsync(PlatformEntity platform, string type)
    {
        var translation = await GetOrAddTranslationAsync(platform, p => p.Translations, platform.Translations);
        translation.Type = type;
    }

    public async Task SavePublisherTranslationAsync(SupplierEntity publisher, string? description)
    {
        var translation = await GetOrAddTranslationAsync(publisher, p => p.Translations, publisher.Translations);
        translation.Description = description;
    }

    public async Task RemoveTranslationsAsync(ProductEntity game)
    {
        await unitOfWork.TranslationRepository.LoadTranslationsAsync(game, g => g.Translations);
        game.Translations.Clear();
    }

    public async Task RemoveTranslationsAsync(CategoryEntity genre)
    {
        await unitOfWork.TranslationRepository.LoadTranslationsAsync(genre, g => g.Translations);
        genre.Translations.Clear();
    }

    public async Task RemoveTranslationsAsync(SupplierEntity publisher)
    {
        await unitOfWork.TranslationRepository.LoadTranslationsAsync(publisher, p => p.Translations);
        publisher.Translations.Clear();
    }

    private static List<Guid> GetIds<T>(IEnumerable<T> items, Func<T, Guid> id)
    {
        return items.Select(id).Distinct().ToList();
    }

    private async Task<TTranslation> GetOrAddTranslationAsync<TEntity, TTranslation>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TTranslation>>> navigation,
        ICollection<TTranslation> translations)
        where TEntity : class
        where TTranslation : TranslationBaseEntity, new()
    {
        await unitOfWork.TranslationRepository.LoadTranslationsAsync(entity, navigation);

        var translation = translations.FirstOrDefault(t => t.LanguageCode == currentLanguage.Code);

        if (translation is null)
        {
            translation = new TTranslation { LanguageCode = currentLanguage.Code };
            translations.Add(translation);
        }

        return translation;
    }
}
