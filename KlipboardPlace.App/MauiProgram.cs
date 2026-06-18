using System.Reflection;
using KlipboardPlace.App.Constants;
using KlipboardPlace.App.ViewModels;
using KlipboardPlace.App.Views;
using KlipboardPlace.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KlipboardPlace.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont(AppFonts.OpenSansRegularFile, AppFonts.OpenSansRegular);
                fonts.AddFont(AppFonts.OpenSansSemiboldFile, AppFonts.OpenSansSemibold);
            });

        var assembly = Assembly.GetExecutingAssembly();

        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(n => n.EndsWith(AppConfiguration.AppSettingsFileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException(
                $"{AppConfiguration.AppSettingsFileName} was not found as an embedded resource. " +
                "Ensure it is marked <EmbeddedResource> in the .csproj.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException(
                               $"Could not open embedded resource stream for '{resourceName}'.");

        builder.Configuration.AddJsonStream(stream);

#if DEBUG
        builder.Configuration.AddUserSecrets(assembly, optional: true);
#endif
        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddKlipboardCore(builder.Configuration);

        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<PlaceDetailsViewModel>();

        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<PlaceDetailsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
