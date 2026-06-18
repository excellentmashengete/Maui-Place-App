using KlipboardPlace.App.Views;

namespace KlipboardPlace.App.Constants;

/// <summary>Shell navigation route names. Must match the routes registered in <c>AppShell</c>.</summary>
public static class Routes
{
    // nameof keeps this in sync with the page type while still being a compile-time constant.
    public const string PlaceDetails = nameof(PlaceDetailsPage);
}

/// <summary>Shell navigation query-parameter keys (shared by the sender and the [QueryProperty] target).</summary>
public static class NavigationKeys
{
    public const string PlaceId = "placeId";
}

/// <summary>App configuration constants.</summary>
public static class AppConfiguration
{
    public const string AppSettingsFileName = "appsettings.json";
}

/// <summary>Registered font file names and the aliases used to reference them.</summary>
public static class AppFonts
{
    public const string OpenSansRegularFile = "OpenSans-Regular.ttf";
    public const string OpenSansRegular = "OpenSansRegular";
    public const string OpenSansSemiboldFile = "OpenSans-Semibold.ttf";
    public const string OpenSansSemibold = "OpenSansSemibold";
}
