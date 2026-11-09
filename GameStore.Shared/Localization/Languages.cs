namespace GameStore.Shared.Localization;

/// <summary>
/// Languages of the store content. English is the default one and is stored in the main tables.
/// </summary>
public static class Languages
{
    public const string English = "en";
    public const string Ukrainian = "uk";
    public const string Polish = "pl";

    public const string Default = English;

    public const int CodeMaxLength = 10;

    public static IReadOnlyList<string> Supported { get; } = [English, Ukrainian, Polish];
}
