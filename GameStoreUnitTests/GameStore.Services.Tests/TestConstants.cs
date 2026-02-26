using DataAccessLayer.Entities;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public static class TestConstants
{
    #region Game

    // --------------------
    // Game
    // --------------------
    public const string ExistingGameKey = "existing-game-key";
    public const string ExistingGameKey2 = "existing-game-key-2";
    public const string NonExistingGameKey = "non-existing-game-key";

    public static readonly Guid ExistingGameId =
        Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");

    public static readonly Guid ExistingGameId2 =
        Guid.Parse("BBBAAAAA-AAAA-AAAA-AAAA-AAAAAAABBBBB");

    public static readonly Guid NonExistingGameId =
        Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");

    #endregion

    #region Genres

    // --------------------
    // Genre Entities
    // --------------------
    public static readonly GenreEntity ExistingGenre1 = new()
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
        Name = "Action",
    };

    public static readonly GenreEntity ExistingGenre2 = new()
    {
        Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
        Name = "Adventure",
    };

    public static readonly List<GenreEntity> ExistingGenres = [ExistingGenre1, ExistingGenre2];

    // --------------------
    // Genre Ids
    // --------------------
    public static readonly Guid ExistingGenreId = ExistingGenre1.Id;
    public static readonly Guid AnotherExistingGenreId = ExistingGenre2.Id;

    public static readonly Guid NonExistingGenreId =
        Guid.Parse("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE");

    public static readonly List<Guid> ExistingGenreIds =
        [ExistingGenreId, AnotherExistingGenreId];

    public static readonly List<Guid> NonExistingGenreIds =
        [NonExistingGenreId];

    #endregion

    // --------------------
    // Platform Entities
    // --------------------
    public static readonly PlatformEntity ExistingPlatform1 = new()
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
        Type = "PC",
    };

    public static readonly PlatformEntity Platform2 = new()
    {
        Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
        Type = "Xbox",
    };

    public static readonly List<PlatformEntity> ExistingPlatforms =
        [ExistingPlatform1, Platform2];

    // --------------------
    // Platform Ids
    // --------------------
    public static readonly Guid ExistingPlatformId = ExistingPlatform1.Id;
    public static readonly Guid AnotherExistingPlatformId = Platform2.Id;

    public static readonly Guid NonExistingPlatformId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static readonly List<Guid> ExistingPlatformIds =
        [ExistingPlatformId, AnotherExistingPlatformId];

    public static readonly List<Guid> NonExistingPlatformIds =
        [NonExistingPlatformId];

    // --------------------
    // Publisher Ids
    // --------------------

    public static readonly Guid ExistingPublisherId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    public static readonly Guid NonExistingPublisherId =
        Guid.Parse("44444444-4444-4444-4444-444444444444");
}