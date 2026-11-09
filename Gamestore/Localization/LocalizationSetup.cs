using BusinessLogicLayer.Localization;
using GameStore.Shared.Localization;

namespace Gamestore.WebAPI.Localization;

public static class LocalizationSetup
{
    public static IServiceCollection AddGameStoreLocalization(this IServiceCollection services)
    {
        services.AddSingleton<ICurrentLanguage, RequestLanguage>();
        services.AddScoped<IContentLocalizer, ContentLocalizer>();

        return services;
    }

    /// <summary>
    /// The Accept-Language header chooses the language of the data. Numbers and dates keep the English format,
    /// so only the UI culture follows the header.
    /// </summary>
    public static IApplicationBuilder UseGameStoreLocalization(this IApplicationBuilder app)
    {
        var supportedLanguages = Languages.Supported.ToArray();

        return app.UseRequestLocalization(options =>
        {
            options.SetDefaultCulture(Languages.Default);
            options.AddSupportedCultures(Languages.Default);
            options.AddSupportedUICultures(supportedLanguages);
        });
    }
}
