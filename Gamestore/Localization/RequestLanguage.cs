using System.Globalization;
using BusinessLogicLayer.Localization;
using GameStore.Shared.Localization;

namespace Gamestore.WebAPI.Localization;

/// <summary>
/// Reads the language that the request localization middleware chose from the Accept-Language header.
/// </summary>
public class RequestLanguage : ICurrentLanguage
{
    public string Code
    {
        get
        {
            var code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return Languages.Supported.Contains(code) ? code : Languages.Default;
        }
    }
}
