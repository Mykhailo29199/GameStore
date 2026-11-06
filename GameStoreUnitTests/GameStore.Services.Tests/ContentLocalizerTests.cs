using BusinessLogicLayer.Localization;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Models.GameModels;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Entities.GameStoreSQLEntities.Localization;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Localization;
using Moq;
using Xunit;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class ContentLocalizerTests
{
    private static readonly Guid TranslatedId = Guid.NewGuid();
    private static readonly Guid NotTranslatedId = Guid.NewGuid();

    private readonly Mock<ISqlServerUnitOfWork> _unitOfWorkMock = new() { DefaultValue = DefaultValue.Mock };

    [Fact]
    public async Task LocalizeGamesAsync_WhenLanguageIsEnglish_ShouldNotReadTranslations()
    {
        // Arrange
        var localizer = CreateLocalizer(Languages.English);
        var games = new List<GameModel> { new() { Id = TranslatedId, Name = "Space Trip" } };

        // Act
        await localizer.LocalizeGamesAsync(games);

        // Assert
        Assert.Equal("Space Trip", games[0].Name);
        _unitOfWorkMock.Verify(
            x => x.TranslationRepository.GetLocalizedGamesAsync(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<Guid>>()),
            Times.Never);
    }

    [Fact]
    public async Task LocalizeGamesAsync_WhenLanguageIsPolish_ShouldUseValuesOfSqlFunction()
    {
        // Arrange
        var localizer = CreateLocalizer(Languages.Polish);
        var games = new List<GameModel>
        {
            new() { Id = TranslatedId, Name = "Space Trip", Description = "Fly" },
            new() { Id = NotTranslatedId, Name = "Chess", Description = "Play" },
        };

        _unitOfWorkMock.Setup(x => x.TranslationRepository.GetLocalizedGamesAsync(Languages.Polish, It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync([new LocalizedGameEntity { Id = TranslatedId, Name = "Kosmiczna podróż", Description = "Leć", LanguageCode = Languages.Polish }]);

        // Act
        await localizer.LocalizeGamesAsync(games);

        // Assert
        Assert.Equal(("Kosmiczna podróż", "Leć"), (games[0].Name, games[0].Description));
        Assert.Equal(("Chess", "Play"), (games[1].Name, games[1].Description));
    }

    [Fact]
    public async Task LocalizeGenresAsync_WhenTranslationExists_ShouldReplaceOnlyTranslatedNames()
    {
        // Arrange
        var localizer = CreateLocalizer(Languages.Ukrainian);
        var genres = new List<GenreModel> { new() { Id = TranslatedId, Name = "Strategy" }, new() { Id = NotTranslatedId, Name = "Puzzle" } };

        _unitOfWorkMock.Setup(x => x.TranslationRepository.GetCategoryTranslationsAsync(Languages.Ukrainian, It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync([new CategoryTranslationEntity { CategoryId = TranslatedId, Name = "Стратегія", LanguageCode = Languages.Ukrainian }]);

        // Act
        await localizer.LocalizeGenresAsync(genres);

        // Assert
        Assert.Equal(["Стратегія", "Puzzle"], genres.Select(g => g.Name));
    }

    [Fact]
    public async Task LocalizePlatformsAsync_WhenTranslationExists_ShouldReplaceType()
    {
        // Arrange
        var localizer = CreateLocalizer(Languages.Polish);
        var platforms = new List<PlatformModel> { new() { Id = TranslatedId, Type = "Console" } };

        _unitOfWorkMock.Setup(x => x.TranslationRepository.GetPlatformTranslationsAsync(Languages.Polish, It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync([new PlatformTranslationEntity { PlatformId = TranslatedId, Type = "Konsola", LanguageCode = Languages.Polish }]);

        // Act
        await localizer.LocalizePlatformsAsync(platforms);

        // Assert
        Assert.Equal("Konsola", platforms[0].Type);
    }

    [Fact]
    public async Task LocalizePublishersAsync_WhenTranslationHasNoDescription_ShouldKeepEnglishDescription()
    {
        // Arrange
        var localizer = CreateLocalizer(Languages.Polish);
        var publishers = new List<PublisherModel> { new() { Id = TranslatedId, CompanyName = "Studio", Description = "English" } };

        _unitOfWorkMock.Setup(x => x.TranslationRepository.GetSupplierTranslationsAsync(Languages.Polish, It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync([new SupplierTranslationEntity { SupplierId = TranslatedId, Description = null, LanguageCode = Languages.Polish }]);

        // Act
        await localizer.LocalizePublishersAsync(publishers);

        // Assert
        Assert.Equal("English", publishers[0].Description);
    }

    [Fact]
    public async Task SaveGameTranslationAsync_WhenGameHasNoTranslation_ShouldAddOneForCurrentLanguage()
    {
        // Arrange
        var localizer = CreateLocalizer(Languages.Ukrainian);
        var game = new ProductEntity { Id = TranslatedId, ProductName = "Space Trip" };

        // Act
        await localizer.SaveGameTranslationAsync(game, "Космічна подорож", "Політ");

        // Assert
        var translation = Assert.Single(game.Translations);
        Assert.Equal((Languages.Ukrainian, "Космічна подорож", "Політ"), (translation.LanguageCode, translation.Name, translation.Description));
        Assert.Equal("Space Trip", game.ProductName);
    }

    [Fact]
    public async Task SaveGameTranslationAsync_WhenTranslationExists_ShouldUpdateItAndKeepOtherLanguages()
    {
        // Arrange
        var localizer = CreateLocalizer(Languages.Polish);
        var game = new ProductEntity { Id = TranslatedId };
        game.Translations.Add(new ProductTranslationEntity { LanguageCode = Languages.Polish, Name = "Stara nazwa" });
        game.Translations.Add(new ProductTranslationEntity { LanguageCode = Languages.Ukrainian, Name = "Назва" });

        // Act
        await localizer.SaveGameTranslationAsync(game, "Nowa nazwa", null);

        // Assert
        Assert.Equal(2, game.Translations.Count);
        Assert.Equal("Nowa nazwa", game.Translations.Single(t => t.LanguageCode == Languages.Polish).Name);
        Assert.Equal("Назва", game.Translations.Single(t => t.LanguageCode == Languages.Ukrainian).Name);
    }

    [Fact]
    public async Task RemoveTranslationsAsync_ShouldLoadAndRemoveAllLanguages()
    {
        // Arrange
        var localizer = CreateLocalizer(Languages.English);
        var genre = new CategoryEntity { Id = TranslatedId };
        genre.Translations.Add(new CategoryTranslationEntity { LanguageCode = Languages.Polish, Name = "Strategia" });
        genre.Translations.Add(new CategoryTranslationEntity { LanguageCode = Languages.Ukrainian, Name = "Стратегія" });

        // Act
        await localizer.RemoveTranslationsAsync(genre);

        // Assert
        Assert.Empty(genre.Translations);
        _unitOfWorkMock.Verify(
            x => x.TranslationRepository.LoadTranslationsAsync(genre, It.IsAny<System.Linq.Expressions.Expression<Func<CategoryEntity, IEnumerable<CategoryTranslationEntity>>>>()),
            Times.Once);
    }

    private ContentLocalizer CreateLocalizer(string languageCode)
    {
        return TestLocalization.CreateLocalizer(_unitOfWorkMock, languageCode);
    }
}
