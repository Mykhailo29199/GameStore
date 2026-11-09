namespace BusinessLogicLayer.Localization;

/// <summary>
/// The language of the current request, one of <see cref="GameStore.Shared.Localization.Languages.Supported"/>.
/// </summary>
public interface ICurrentLanguage
{
    string Code { get; }
}
