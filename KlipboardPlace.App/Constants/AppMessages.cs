namespace KlipboardPlace.App.Constants;

/// <summary>
/// User-facing messages, centralised so wording is managed from one place.
/// The "*Format" entries are composite format strings — use with <see cref="string.Format(string, object?)"/>.
/// </summary>
public static class AppMessages
{
    // Empty-state
    public const string NoPlacesFound = "No places found. Try a different search.";
    public const string NoPlaceDetailsFound = "No details were found for this place.";

    // Errors ({0} = underlying detail)
    public const string SearchFailedFormat = "Could not complete the search: {0}";
    public const string PlaceDetailsLoadFailedFormat = "Could not load place details: {0}";
    public const string OpenMapFailedFormat = "Could not open the map: {0}";
}
